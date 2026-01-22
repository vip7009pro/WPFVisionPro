using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Flow/pipeline definition
/// </summary>
public class FlowDefinition
{
    [JsonPropertyName("flowId")]
    public string FlowId { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Default Flow";
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
    
    [JsonPropertyName("nodes")]
    public List<FlowNodeDefinition> Nodes { get; set; } = new();
    
    [JsonPropertyName("connections")]
    public List<FlowConnection> Connections { get; set; } = new();
}

/// <summary>
/// Definition of a single flow node (for serialization)
/// </summary>
public class FlowNodeDefinition
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("nodeType")]
    public FlowNodeType NodeType { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("positionX")]
    public double PositionX { get; set; }
    
    [JsonPropertyName("positionY")]
    public double PositionY { get; set; }
    
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = new();
    
    [JsonPropertyName("outputs")]
    public List<string> Outputs { get; set; } = new();
    
    [JsonPropertyName("config")]
    public Dictionary<string, object> Config { get; set; } = new();
}

/// <summary>
/// Connection between two flow nodes
/// </summary>
public class FlowConnection
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("sourceNodeId")]
    public string SourceNodeId { get; set; } = string.Empty;
    
    [JsonPropertyName("sourcePort")]
    public string SourcePort { get; set; } = "output";
    
    [JsonPropertyName("targetNodeId")]
    public string TargetNodeId { get; set; } = string.Empty;
    
    [JsonPropertyName("targetPort")]
    public string TargetPort { get; set; } = "input";
}
