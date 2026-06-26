using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orbit.Engine.Engine;

/// <summary>
/// Loads hotkey → workflow mappings from orbit.yaml.
/// Expected format:
///   hotkeys:
///     ctrl+alt+t:
///       workflow: my-workflow
/// </summary>
public static class HotkeyConfigLoader
{
    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Returns a dictionary of hotkey string → workflow name.
    /// Returns empty dictionary if the file is missing or has no hotkeys section.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Load(string configPath)
    {
        if (!File.Exists(configPath))
            return new Dictionary<string, string>();

        var yaml = File.ReadAllText(configPath);
        var raw = _deserializer.Deserialize<Dictionary<string, object>>(yaml);

        if (!raw.TryGetValue("hotkeys", out var hotkeysObj) ||
            hotkeysObj is not Dictionary<object, object> hotkeysDict)
            return new Dictionary<string, string>();

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (keyObj, valueObj) in hotkeysDict)
        {
            var hotkey = keyObj?.ToString();
            if (hotkey is null) continue;

            if (valueObj is Dictionary<object, object> entry &&
                entry.TryGetValue("workflow", out var wf) &&
                wf?.ToString() is string workflowName)
            {
                result[hotkey] = workflowName;
            }
        }

        return result;
    }
}
