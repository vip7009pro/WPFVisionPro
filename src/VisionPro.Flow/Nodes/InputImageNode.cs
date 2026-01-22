using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes.Base;

namespace VisionPro.Flow.Nodes;

/// <summary>
/// Input image source node - starting point of every flow
/// </summary>
public class InputImageNode : FlowNodeBase
{
    public override FlowNodeType NodeType => FlowNodeType.InputImage;
    
    public InputImageNode()
    {
        Name = "Input Image";
    }
    
    public override async Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            if (context.CurrentImage == null)
            {
                Result = FlowNodeResult.CreateFailure(NodeId, "No input image available");
                return Result;
            }
            
            InputImage = context.CurrentImage;
            OutputImage = context.CurrentImage;
            IsExecuted = true;
            
            stopwatch.Stop();
            
            Result = new FlowNodeResult
            {
                NodeId = NodeId,
                Success = true,
                Status = InspectionStatus.OK,
                OutputImage = OutputImage,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Data =
                {
                    ["width"] = context.ImageWidth,
                    ["height"] = context.ImageHeight
                }
            };
            
            return Result;
        }, cancellationToken);
    }
}
