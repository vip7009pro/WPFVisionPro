using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using VisionPro.Core.Models;

namespace VisionPro.App.ViewModels;

public partial class FlowConnectionViewModel : ObservableObject
{
    private readonly FlowNodeViewModel _sourceNode;
    private readonly FlowNodeViewModel _targetNode;
    private readonly string _sourcePortName;
    private readonly string _targetPortName;

    [ObservableProperty]
    private Point startPoint;

    [ObservableProperty]
    private Point endPoint;

    [ObservableProperty]
    private Point controlPoint1;

    [ObservableProperty]
    private Point controlPoint2;

    public FlowConnection Model { get; }

    public FlowConnectionViewModel(FlowConnection model, FlowNodeViewModel sourceNode, FlowNodeViewModel targetNode)
    {
        Model = model;
        _sourceNode = sourceNode;
        _targetNode = targetNode;
        _sourcePortName = model.SourcePort;
        _targetPortName = model.TargetPort;

        // Listen to node moves to update connection
        _sourceNode.PropertyChanged += Node_PropertyChanged;
        _targetNode.PropertyChanged += Node_PropertyChanged;
        
        UpdateGeometry();
    }

    private void Node_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FlowNodeViewModel.X) || e.PropertyName == nameof(FlowNodeViewModel.Y))
        {
            UpdateGeometry();
        }
    }

    public void UpdateGeometry()
    {
        // Simple heuristic for port positions
        // Outputs are on Right, Inputs on Left
        
        // Find port index
        int sourceIndex = _sourceNode.OutputPorts.TakeWhile(p => p.Name != _sourcePortName).Count();
        if (sourceIndex == _sourceNode.OutputPorts.Count) sourceIndex = 0; // Default

        int targetIndex = _targetNode.InputPorts.TakeWhile(p => p.Name != _targetPortName).Count();
        if (targetIndex == _targetNode.InputPorts.Count) targetIndex = 0; // Default

        // Constants matching XAML layout approximately
        double portHeight = 20;
        double headerHeight = 30; // Approx
        double nodeWidthMin = 140; 
        
        // Source (Right side)
        double startX = _sourceNode.X + nodeWidthMin; 
        double startY = _sourceNode.Y + headerHeight + (sourceIndex * portHeight) + 10;

        // Target (Left side)
        double endX = _targetNode.X;
        double endY = _targetNode.Y + headerHeight + (targetIndex * portHeight) + 10;

        StartPoint = new Point(startX, startY);
        EndPoint = new Point(endX, endY);

        // Bezier Control Points
        double dist = Math.Abs(endX - startX) / 2;
        if (dist < 50) dist = 50;

        ControlPoint1 = new Point(startX + dist, startY);
        ControlPoint2 = new Point(endX - dist, endY);
    }
}
