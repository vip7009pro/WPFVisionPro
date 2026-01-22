using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;

namespace VisionPro.Flow.Nodes.Base;

/// <summary>
/// Base class for all flow nodes providing common functionality
/// </summary>
public abstract class FlowNodeBase : IFlowNode
{
    protected JsonElement _configuration;
    
    public string NodeId { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Flow Node";
    public abstract FlowNodeType NodeType { get; }
    public List<string> InputNodeIds { get; } = new();
    public List<string> OutputNodeIds { get; } = new();
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public byte[]? InputImage { get; protected set; }
    public byte[]? OutputImage { get; protected set; }
    public FlowNodeResult? Result { get; protected set; }
    public bool IsExecuted { get; protected set; }
    
    public virtual void Configure(JsonElement configuration)
    {
        _configuration = configuration;
        
        if (configuration.TryGetProperty("name", out var nameProp))
            Name = nameProp.GetString() ?? Name;
        
        if (configuration.TryGetProperty("positionX", out var xProp))
            PositionX = xProp.GetDouble();
        
        if (configuration.TryGetProperty("positionY", out var yProp))
            PositionY = yProp.GetDouble();
        
        if (configuration.TryGetProperty("inputs", out var inputsProp))
        {
            InputNodeIds.Clear();
            foreach (var input in inputsProp.EnumerateArray())
                InputNodeIds.Add(input.GetString() ?? "");
        }
        
        if (configuration.TryGetProperty("outputs", out var outputsProp))
        {
            OutputNodeIds.Clear();
            foreach (var output in outputsProp.EnumerateArray())
                OutputNodeIds.Add(output.GetString() ?? "");
        }
        
        OnConfigure(configuration);
    }
    
    protected virtual void OnConfigure(JsonElement configuration) { }
    
    public abstract Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default);
    
    public virtual ValidationResult Validate()
    {
        return ValidationResult.Valid();
    }
    
    public virtual void Reset()
    {
        InputImage = null;
        OutputImage = null;
        Result = null;
        IsExecuted = false;
    }
    
    public virtual JsonElement GetConfiguration()
    {
        return _configuration;
    }
}
