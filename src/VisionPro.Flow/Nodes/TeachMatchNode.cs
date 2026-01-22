using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes.Base;
using VisionPro.Vision.Tools;

namespace VisionPro.Flow.Nodes;

/// <summary>
/// Template matching node using ORB/AKAZE feature detection
/// </summary>
public class TeachMatchNode : FlowNodeBase
{
    private readonly TeachMatchTool _tool = new();
    
    public override FlowNodeType NodeType => FlowNodeType.TeachMatch;
    
    public TeachMatchTool Tool => _tool;
    
    public TeachMatchNode()
    {
        Name = "Teach Match";
    }
    
    protected override void OnConfigure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("config", out var configProp))
        {
            _tool.Configure(configProp);
        }
    }
    
    public override async Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default)
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
        
        if (inputImage == null)
        {
            inputImage = context.CurrentImage;
        }
        
        if (inputImage == null)
        {
            Result = FlowNodeResult.CreateFailure(NodeId, "No input image available");
            return Result;
        }
        
        InputImage = inputImage;
        
        // Execute the tool
        var toolResult = await _tool.ExecuteAsync(inputImage, context.ImageWidth, context.ImageHeight, null, cancellationToken);
        
        OutputImage = toolResult.OutputImage ?? inputImage;
        IsExecuted = true;
        
        // Store position in context for other nodes
        if (toolResult.Position != null)
        {
            context.ProductPosition = toolResult.Position;
        }
        
        stopwatch.Stop();
        
        Result = new FlowNodeResult
        {
            NodeId = NodeId,
            Success = toolResult.Success,
            Status = toolResult.Status,
            OutputImage = OutputImage,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            Data = toolResult.Data,
            ErrorMessage = toolResult.ErrorMessage
        };
        
        return Result;
    }
    
    public override ValidationResult Validate()
    {
        if (!_tool.IsTaught)
        {
            return ValidationResult.Invalid("Pattern has not been taught");
        }
        return ValidationResult.Valid();
    }
}
