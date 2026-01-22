using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes.Base;

namespace VisionPro.Flow.Nodes;

/// <summary>
/// Final decision node - determines final OK/NG result
/// </summary>
public class FinalDecisionNode : FlowNodeBase
{
    private string _logic = "AND"; // AND or OR
    private List<string> _conditions = new();
    
    public override FlowNodeType NodeType => FlowNodeType.FinalDecision;
    
    public FinalDecisionNode() : base("Final Decision")
    {
    }

    protected override void InitializePorts()
    {
        InputPorts.Add(new Ports.NodePort("Decision In", Ports.PortType.Data, Ports.PortDirection.Input, NodeId));
    }
    
    protected override void OnConfigure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("config", out var config))
        {
            if (config.TryGetProperty("logic", out var logicProp))
                _logic = logicProp.GetString() ?? "AND";
            
            if (config.TryGetProperty("conditions", out var condProp))
            {
                _conditions.Clear();
                foreach (var cond in condProp.EnumerateArray())
                    _conditions.Add(cond.GetString() ?? "");
            }
        }
    }
    
    public override async Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Get input image from previous node
            byte[]? inputImage = null;
            foreach (var inputNodeId in InputNodeIds)
            {
                if (context.NodeResults.TryGetValue(inputNodeId, out var inputResult) && inputResult.OutputImage != null)
                {
                    inputImage = inputResult.OutputImage;
                    break;
                }
            }
            
            InputImage = inputImage ?? context.CurrentImage;
            OutputImage = InputImage;
            
            // Evaluate all input node results
            var inputStatuses = new List<InspectionStatus>();
            
            foreach (var inputNodeId in InputNodeIds)
            {
                if (context.NodeResults.TryGetValue(inputNodeId, out var nodeResult))
                {
                    inputStatuses.Add(nodeResult.Status);
                }
            }
            
            // Apply logic
            InspectionStatus finalStatus;
            
            if (inputStatuses.Count == 0)
            {
                finalStatus = InspectionStatus.NotInspected;
            }
            else if (_logic.ToUpperInvariant() == "AND")
            {
                // All must be OK
                finalStatus = inputStatuses.All(s => s == InspectionStatus.OK)
                    ? InspectionStatus.OK
                    : InspectionStatus.NG;
            }
            else // OR
            {
                // At least one must be OK
                finalStatus = inputStatuses.Any(s => s == InspectionStatus.OK)
                    ? InspectionStatus.OK
                    : InspectionStatus.NG;
            }
            
            // Check for errors
            var hasError = inputStatuses.Any(s => s == InspectionStatus.Error);
            if (hasError)
            {
                finalStatus = InspectionStatus.Error;
            }
            
            IsExecuted = true;
            stopwatch.Stop();
            
            Result = new FlowNodeResult
            {
                NodeId = NodeId,
                Success = true,
                Status = finalStatus,
                OutputImage = OutputImage,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Data =
                {
                    ["logic"] = _logic,
                    ["inputCount"] = inputStatuses.Count,
                    ["okCount"] = inputStatuses.Count(s => s == InspectionStatus.OK),
                    ["ngCount"] = inputStatuses.Count(s => s == InspectionStatus.NG)
                }
            };
            
            return Result;
        }, cancellationToken);
    }
}
