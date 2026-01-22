using VisionPro.Core.Enums;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes;

namespace VisionPro.Flow.Engine;

/// <summary>
/// Main flow execution engine that processes node pipelines
/// </summary>
public class FlowEngine
{
    private readonly Dictionary<string, IFlowNode> _nodes = new();
    private FlowDefinition? _flowDefinition;
    
    public event EventHandler<FlowNodeResult>? NodeExecuted;
    public event EventHandler<InspectionResult>? FlowCompleted;
    public event EventHandler<Exception>? ErrorOccurred;
    
    /// <summary>
    /// Load a flow definition
    /// </summary>
    public void LoadFlow(FlowDefinition flow)
    {
        _flowDefinition = flow;
        _nodes.Clear();
        
        var serializer = new Serialization.FlowSerializer();
        var nodes = serializer.CreateNodes(flow);
        
        foreach (var node in nodes)
        {
            _nodes[node.NodeId] = node;
        }
    }
    
    /// <summary>
    /// Get current flow definition
    /// </summary>
    public FlowDefinition GetDefinition()
    {
        var serializer = new Serialization.FlowSerializer();
        var name = _flowDefinition?.Name ?? "Current Flow";
        return serializer.ConvertToDefinition(_nodes.Values.ToList(), name);
    }
    
    /// <summary>
    /// Execute the flow with the provided image
    /// </summary>
    public async Task<InspectionResult> ExecuteAsync(
        byte[] imageData, 
        int width, 
        int height,
        ProductConfig? productConfig = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        var context = new FlowExecutionContext
        {
            CurrentImage = imageData,
            ImageWidth = width,
            ImageHeight = height,
            ROIs = productConfig?.ROIs,
            Thresholds = productConfig?.Thresholds,
            CancellationToken = cancellationToken,
            StartTime = startTime
        };
        
        try
        {
            // Reset all nodes
            foreach (var node in _nodes.Values)
            {
                node.Reset();
            }
            
            // Find entry points (nodes with no inputs)
            var entryNodes = _nodes.Values
                .Where(n => n.InputNodeIds.Count == 0)
                .ToList();
            
            if (entryNodes.Count == 0)
            {
                throw new InvalidOperationException("No entry nodes found in flow");
            }
            
            // Execute nodes in topological order
            var executedNodes = new HashSet<string>();
            var nodesToExecute = new Queue<IFlowNode>(entryNodes);
            
            while (nodesToExecute.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var node = nodesToExecute.Dequeue();
                
                // Check if all inputs are ready
                if (!node.InputNodeIds.All(id => executedNodes.Contains(id)))
                {
                    // Re-queue and continue
                    nodesToExecute.Enqueue(node);
                    continue;
                }
                
                // Execute node
                var result = await node.ExecuteAsync(context, cancellationToken);
                context.NodeResults[node.NodeId] = result;
                executedNodes.Add(node.NodeId);
                
                NodeExecuted?.Invoke(this, result);
                
                // Queue output nodes
                foreach (var outputId in node.OutputNodeIds)
                {
                    if (_nodes.TryGetValue(outputId, out var outputNode) && !executedNodes.Contains(outputId))
                    {
                        if (!nodesToExecute.Contains(outputNode))
                        {
                            nodesToExecute.Enqueue(outputNode);
                        }
                    }
                }
            }
            
            // Find final decision node result
            var finalNode = _nodes.Values.FirstOrDefault(n => n.NodeType == FlowNodeType.FinalDecision);
            var finalStatus = finalNode?.Result?.Status ?? InspectionStatus.NotInspected;
            
            var inspectionResult = new InspectionResult
            {
                ProductId = productConfig?.Product.ProductId ?? "",
                Status = finalStatus,
                Timestamp = startTime,
                DurationMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                Measurements = context.Measurements,
                Defects = context.Defects,
                ProductPosition = context.ProductPosition
            };
            
            FlowCompleted?.Invoke(this, inspectionResult);
            return inspectionResult;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            
            return new InspectionResult
            {
                ProductId = productConfig?.Product.ProductId ?? "",
                Status = InspectionStatus.Error,
                Timestamp = startTime,
                DurationMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }
    
    /// <summary>
    /// Get all nodes in the flow
    /// </summary>
    public IEnumerable<IFlowNode> GetNodes() => _nodes.Values;
    
    /// <summary>
    /// Get a specific node by ID
    /// </summary>
    public IFlowNode? GetNode(string nodeId) => _nodes.GetValueOrDefault(nodeId);
    
    /// <summary>
    /// Validate the flow
    /// </summary>
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (_nodes.Count == 0)
        {
            result.AddError("Flow has no nodes");
            return result;
        }
        
        // Check for entry points
        var entryNodes = _nodes.Values.Where(n => n.InputNodeIds.Count == 0).ToList();
        if (entryNodes.Count == 0)
        {
            result.AddError("Flow has no entry nodes (nodes with no inputs)");
        }
        
        // Check for final decision
        var finalNodes = _nodes.Values.Where(n => n.NodeType == FlowNodeType.FinalDecision).ToList();
        if (finalNodes.Count == 0)
        {
            result.AddWarning("Flow has no FinalDecision node");
        }
        
        // Validate each node
        foreach (var node in _nodes.Values)
        {
            var nodeValidation = node.Validate();
            if (!nodeValidation.IsValid)
            {
                foreach (var error in nodeValidation.Errors)
                {
                    result.AddError($"Node '{node.Name}': {error}");
                }
            }
        }
        
        return result;
    }
    

}
