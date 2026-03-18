using Mabean.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Mabean.Builders
{
    internal class VisualizationNodeBuilder
    {
        private string _type = string.Empty;

        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public VisualizationNodeBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public VisualizationNode? Build(string libraryPath)
        {
            string json = File.ReadAllText(libraryPath);

            using var doc = JsonDocument.Parse(json);

            var stepsElement = _type switch
            {
                "Injection-Simple" => doc.RootElement
                    .GetProperty("Behavior-Description")
                    .GetProperty("Simple-Injection")
                    .GetProperty("Steps"),
                _ => default
            };

            if (stepsElement.ValueKind == JsonValueKind.Undefined)
                return null;

            var steps = stepsElement.Deserialize<List<VisualizationNode>>(_options);

            if (steps is null || steps.Count == 0)
                return null;

            for (int i = 0; i < steps.Count - 1; i++)
                steps[i].Next = steps[i + 1];

            return steps[0];
        }
    }
}
