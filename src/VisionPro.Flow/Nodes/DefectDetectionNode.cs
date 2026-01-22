using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Flow.Nodes.Base;

namespace VisionPro.Flow.Nodes;

public class DefectDetectionNode : FlowNodeBase
{
    public override FlowNodeType NodeType => FlowNodeType.DefectDetection;

    public DefectDetectionNode() : base("Defect Detection")
    {
    }

    protected override void InitializePorts()
    {
        InputPorts.Add(new Ports.NodePort("Image In", Ports.PortType.Image, Ports.PortDirection.Input, NodeId));
        OutputPorts.Add(new Ports.NodePort("Image Out", Ports.PortType.Image, Ports.PortDirection.Output, NodeId));
        OutputPorts.Add(new Ports.NodePort("Defects", Ports.PortType.Data, Ports.PortDirection.Output, NodeId));
    }

    public override Task<FlowNodeResult> ExecuteAsync(FlowExecutionContext context, CancellationToken cancellationToken = default)
    {
        InputImage = context.CurrentImage;
        OutputImage = InputImage;
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
        // TODO: Configure defect tools
    }
}
