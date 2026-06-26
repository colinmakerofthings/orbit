using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Runtime.InteropServices;

namespace Orbit.Actions;

public class TimestampTextAction : IAction
{
    public string Name => "timestamp-text";

    public Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var template = parameters.TryGetValue("template", out var t) && t is string tmpl
            ? tmpl
            : "{yyyy}{MM}{dd}{HH}{mm}{ss}";

        var now = DateTime.Now;

        // Replace tokens like {HH}, {mm}, {ss}, {yyyy}, {MM}, {dd}
        var text = template
            .Replace("{yyyy}", now.ToString("yyyy"))
            .Replace("{yy}",   now.ToString("yy"))
            .Replace("{MM}",   now.ToString("MM"))
            .Replace("{dd}",   now.ToString("dd"))
            .Replace("{HH}",   now.ToString("HH"))
            .Replace("{mm}",   now.ToString("mm"))
            .Replace("{ss}",   now.ToString("ss"));

        try
        {
            SendKeys(text);
            return Task.FromResult(ActionResult.Ok($"Typed timestamp: {text}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Fail($"Failed to type timestamp: {ex.Message}"));
        }
    }

    private static void SendKeys(string text)
    {
        var inputs = new List<INPUT>();

        foreach (var ch in text)
        {
            inputs.Add(MakeCharInput(ch, false));
            inputs.Add(MakeCharInput(ch, true));
        }

        var arr = inputs.ToArray();
        SendInput((uint)arr.Length, arr, Marshal.SizeOf<INPUT>());
    }

    private static INPUT MakeCharInput(char ch, bool keyUp) => new()
    {
        type = INPUT_KEYBOARD,
        u = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = ch,
                dwFlags = KEYEVENTF_UNICODE | (keyUp ? KEYEVENTF_KEYUP : 0u),
                time = 0,
                dwExtraInfo = 0
            }
        }
    };

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }
}
