using Orbit.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orbit.Engine.Engine;

public class WorkflowLoader(string workflowsDirectory)
{
    // Deserialize the top-level workflow, but steps as raw dicts to capture arbitrary params
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public Workflow? Load(string workflowName)
    {
        var path = Path.Combine(workflowsDirectory, $"{workflowName}.yaml");
        if (!File.Exists(path))
            path = Path.Combine(workflowsDirectory, $"{workflowName}.yml");

        if (!File.Exists(path))
            return null;

        return ParseFile(path);
    }

    public IReadOnlyList<Workflow> LoadAll()
    {
        if (!Directory.Exists(workflowsDirectory))
            return [];

        return Directory
            .GetFiles(workflowsDirectory, "*.yaml")
            .Concat(Directory.GetFiles(workflowsDirectory, "*.yml"))
            .Select(ParseFile)
            .ToList();
    }

    private Workflow ParseFile(string path)
    {
        var yaml = File.ReadAllText(path);

        // Deserialize as a raw dictionary to preserve all step parameters
        var raw = _deserializer.Deserialize<Dictionary<string, object>>(yaml);

        var name = raw.TryGetValue("name", out var n) ? n?.ToString() : null;
        if (name is null)
            throw new InvalidOperationException($"Workflow file '{path}' is missing 'name'.");

        var description = raw.TryGetValue("description", out var d) ? d?.ToString() : null;

        var steps = new List<WorkflowStep>();

        if (raw.TryGetValue("steps", out var stepsObj) && stepsObj is List<object> stepList)
        {
            foreach (var stepObj in stepList)
            {
                if (stepObj is not Dictionary<object, object> stepDict)
                    continue;

                // Normalise keys to string
                var stepParams = stepDict.ToDictionary(
                    kv => kv.Key.ToString()!,
                    kv => kv.Value);

                if (!stepParams.TryGetValue("action", out var actionObj))
                    throw new InvalidOperationException($"A step in '{name}' is missing the 'action' key.");

                var actionName = actionObj?.ToString()
                    ?? throw new InvalidOperationException($"A step in '{name}' has a null 'action' value.");

                // Everything except 'action' is a parameter
                var parameters = stepParams
                    .Where(kv => kv.Key != "action")
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                steps.Add(new WorkflowStep
                {
                    Action = actionName,
                    Parameters = parameters
                });
            }
        }

        return new Workflow
        {
            Name = name,
            Description = description,
            Steps = steps
        };
    }
}
