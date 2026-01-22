using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes.Base;

namespace VisionPro.Flow.Nodes;

public class ROIApplyNode : FlowNodeBase
{
    public override FlowNodeType NodeType => FlowNodeType.ROIApply;
    
    // Config: Which ROI ID to apply
    private string _roiId = string.Empty;

    public ROIApplyNode() : base("Apply ROI")
    {
    }

    protected override void InitializePorts()
    {
        InputPorts.Add(new Ports.NodePort("Image In", Ports.PortType.Image, Ports.PortDirection.Input, NodeId));
        OutputPorts.Add(new Ports.NodePort("Image Out", Ports.PortType.Image, Ports.PortDirection.Output, NodeId));
    }

    public override Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default)
    {
        // For now, just pass through the image but conceptually we would crop or mask it.
        // In the future this node will use context.ROIs to find the ROI and generate a mask.
        
        // TODO: Implement actual masking logic using ROIManager
        
        InputImage = context.CurrentImage;
        OutputImage = InputImage; // Pass-through for now
        IsExecuted = true;
        
        Result = new FlowNodeResult
        {
            NodeId = NodeId,
            Status = InspectionStatus.OK,
            OutputImage = OutputImage
        };
        
        return Task.FromResult(Result);
    }

    public override void Configure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("roiId", out var roiIdProp))
        {
            _roiId = roiIdProp.GetString() ?? string.Empty;
        }
    }
}
