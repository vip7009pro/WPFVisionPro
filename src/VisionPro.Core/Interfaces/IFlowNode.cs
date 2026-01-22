using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;

namespace VisionPro.Core.Interfaces;

/// <summary>
/// Interface for flow pipeline nodes.
/// Each node represents a step in the inspection flow.
/// </summary>
public interface IFlowNode
{
    /// <summary>
    /// Unique identifier for this node
    /// </summary>
    string NodeId { get; }
    
    /// <summary>
    /// Display name of the node
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Type of flow node
    /// </summary>
    FlowNodeType NodeType { get; }
    
    /// <summary>
    /// IDs of nodes that provide input to this node
    /// </summary>
    List<string> InputNodeIds { get; }
    
    /// <summary>
    /// IDs of nodes that receive output from this node
    /// </summary>
    List<string> OutputNodeIds { get; }
    
    /// <summary>
    /// X position in the flow editor canvas
    /// </summary>
    double PositionX { get; set; }
    
    /// <summary>
    /// Y position in the flow editor canvas
    /// </summary>
    double PositionY { get; set; }
    
    /// <summary>
    /// Input image data for this node (set during execution)
    /// </summary>
    byte[]? InputImage { get; }
    
    /// <summary>
    /// Output image data from this node (set during execution)
    /// </summary>
    byte[]? OutputImage { get; }
    
    /// <summary>
    /// Result data from this node's execution
    /// </summary>
    FlowNodeResult? Result { get; }
    
    /// <summary>
    /// Indicates whether this node has been executed in the current flow
    /// </summary>
    bool IsExecuted { get; }
    
    /// <summary>
    /// Configure the node with JSON settings
    /// </summary>
    /// <param name="configuration">JSON configuration</param>
    void Configure(JsonElement configuration);
    
    /// <summary>
    /// Execute this node within the flow context
    /// </summary>
    /// <param name="context">Flow execution context containing shared state</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate that the node is properly configured
    /// </summary>
    /// <returns>Validation result with any error messages</returns>
    ValidationResult Validate();
    
    /// <summary>
    /// Reset the node state for a new execution
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Get the current configuration as JSON
    /// </summary>
    JsonElement GetConfiguration();
}
