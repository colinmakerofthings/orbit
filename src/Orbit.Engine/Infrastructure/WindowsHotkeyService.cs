using Microsoft.Extensions.Logging;
using Orbit.Core.Contracts;
using System.Runtime.InteropServices;

namespace Orbit.Engine.Infrastructure;

/// <summary>
/// Registers Windows global hotkeys using RegisterHotKey/UnregisterHotKey.
/// Must be pumped via a message loop (Run() blocks the calling thread).
/// Designed to run on a dedicated STA thread inside a BackgroundService.
/// </summary>
public class WindowsHotkeyService(ILogger<WindowsHotkeyService> logger) : IHotkeyService, IDisposable
{
    private readonly Dictionary<int, (string hotkey, Func<Task> handler)> _registrations = new();
    private int _nextId = 0xB000;
    private nint _hwnd;
    private Thread? _messageThread;
    private bool _running;

    public void Register(string hotkey, Func<Task> handler)
    {
        var id = _nextId++;
        _registrations[id] = (hotkey, handler);
        logger.LogInformation("Registered hotkey: {Hotkey} (id={Id})", hotkey, id);
    }

    public void Unregister(string hotkey)
    {
        var entry = _registrations.FirstOrDefault(kv => kv.Value.hotkey == hotkey);
        if (entry.Value.hotkey is null) return;

        if (_hwnd != nint.Zero)
            UnregisterHotKey(_hwnd, entry.Key);

        _registrations.Remove(entry.Key);
    }

    public void Start()
    {
        _running = true;
        _messageThread = new Thread(RunMessageLoop) { IsBackground = true };
        _messageThread.SetApartmentState(ApartmentState.STA);
        _messageThread.Start();
    }

    public void Stop()
    {
        _running = false;
        if (_hwnd != nint.Zero)
            PostMessage(_hwnd, WM_QUIT, 0, 0);
    }

    private void RunMessageLoop()
    {
        // Create a message-only window to receive WM_HOTKEY
        _hwnd = CreateMessageWindow();

        foreach (var (id, (hotkey, _)) in _registrations)
        {
            ParseHotkey(hotkey, out var modifiers, out var vk);
            if (!RegisterHotKey(_hwnd, id, modifiers, vk))
                logger.LogWarning("Failed to register hotkey: {Hotkey}", hotkey);
        }

        while (_running && GetMessage(out var msg, nint.Zero, 0, 0))
        {
            if (msg.message == WM_HOTKEY)
            {
                var id = (int)msg.wParam;
                if (_registrations.TryGetValue(id, out var reg))
                {
                    logger.LogInformation("Hotkey triggered: {Hotkey}", reg.hotkey);
                    _ = Task.Run(reg.handler);
                }
            }

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        foreach (var id in _registrations.Keys)
            UnregisterHotKey(_hwnd, id);
    }

    private static void ParseHotkey(string hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        var parts = hotkey.ToLowerInvariant().Split('+');

        foreach (var part in parts)
        {
            switch (part.Trim())
            {
                case "ctrl":  modifiers |= MOD_CONTROL; break;
                case "alt":   modifiers |= MOD_ALT;     break;
                case "shift": modifiers |= MOD_SHIFT;   break;
                case "win":   modifiers |= MOD_WIN;     break;
                default:
                    if (part.Length == 1)
                        vk = (uint)char.ToUpper(part[0]);
                    break;
            }
        }
    }

    private delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);
    private static readonly WndProcDelegate _wndProcDelegate = DefWindowProc;

    private static nint CreateMessageWindow()
    {
        var wndClass = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = GetModuleHandle(null),
            lpszClassName = "OrbitHotkeyWindow"
        };

        RegisterClassEx(ref wndClass);

        return CreateWindowEx(0, "OrbitHotkeyWindow", null, 0,
            0, 0, 0, 0, new nint(-3), nint.Zero, nint.Zero, nint.Zero);
    }

    public void Dispose() => Stop();

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint WM_HOTKEY = 0x0312;
    private const uint WM_QUIT = 0x0012;

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(nint hWnd, int id);
    [DllImport("user32.dll")] private static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    [DllImport("user32.dll")] private static extern bool TranslateMessage(ref MSG lpMsg);
    [DllImport("user32.dll")] private static extern nint DispatchMessage(ref MSG lpmsg);
    [DllImport("user32.dll")] private static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);
    [DllImport("user32.dll")] private static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);
    [DllImport("user32.dll")] private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
    [DllImport("user32.dll")] private static extern nint CreateWindowEx(uint dwExStyle, string lpClassName, string? lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);
    [DllImport("kernel32.dll")] private static extern nint GetModuleHandle(string? lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public int cbSize;
        public uint style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public nint hIconSm;
    }
}
