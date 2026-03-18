using System;
using System.Collections.Generic;
using System.Text;

namespace Mabean.Models
{
    public class VisualizationNode
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required VisualizationNodeFunctionSignature Signature { get; set; }
        public VisualizationNode? Next { get; set; }

        public VisualizationNode GetLast()
        {
            for(VisualizationNode current = this; ; current = current.Next!)
            {
                if (current.Next is null)
                {
                    return current;
                }
            }
        }

    }

    public class VisualizationNodeFunctionSignature
    {
        public required string FunctionName { get; set; }
        public List<VisualizationNodeFunctionParameter> Parameters { get; set; } = [];

    }

    public class VisualizationNodeFunctionParameter
    {
        public required string Name {  set; get; }
        public required string Description { set; get; }
        public required string Value { set; get; }
    }
}
