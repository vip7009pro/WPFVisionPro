using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionPro.Core.Enums;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;
using VisionPro.Flow.Engine;

namespace VisionPro.App.ViewModels;

/// <summary>
/// ViewModel for Flow Editor screen
/// </summary>
public partial class FlowEditorViewModel : ObservableObject
{
    private readonly FlowEngine _flowEngine;
    
    [ObservableProperty]
    private ObservableCollection<FlowNodeViewModel> nodes = new();
    
    [ObservableProperty]
    private FlowNodeViewModel? selectedNode;
    
    [ObservableProperty]
    private string flowName = "New Flow";
    
    [ObservableProperty]
    private bool isModified;
    
    public FlowEditorViewModel(FlowEngine flowEngine)
    {
        _flowEngine = flowEngine;
    }
    
    [RelayCommand]
    private void AddNode(FlowNodeType nodeType)
    {
        var node = new FlowNodeViewModel
        {
            NodeType = nodeType,
            Name = GetDefaultNodeName(nodeType),
            X = 100 + Nodes.Count * 50,
            Y = 100 + Nodes.Count * 30
        };
        
        Nodes.Add(node);
        SelectedNode = node;
        IsModified = true;
    }
    
    [RelayCommand]
    private void RemoveSelectedNode()
    {
        if (SelectedNode != null)
        {
            Nodes.Remove(SelectedNode);
            SelectedNode = null;
            IsModified = true;
        }
    }
    
    [RelayCommand]
    private void SaveFlow()
    {
        // TODO: Implement flow serialization
        IsModified = false;
    }
    
    [RelayCommand]
    private void LoadFlow()
    {
        // TODO: Implement flow loading
    }
    
    private static string GetDefaultNodeName(FlowNodeType nodeType) => nodeType switch
    {
        FlowNodeType.InputImage => "Input Image",
        FlowNodeType.TeachMatch => "Teach Match",
        FlowNodeType.ROIApply => "Apply ROI",
        FlowNodeType.Measurement => "Measurement",
        FlowNodeType.ThresholdCompare => "Threshold",
        FlowNodeType.ConditionalBranch => "Condition",
        FlowNodeType.FinalDecision => "Final Decision",
        FlowNodeType.DefectDetection => "Defect Detection",
        _ => "Node"
    };
}

/// <summary>
/// ViewModel for a single flow node in the editor
/// </summary>
public partial class FlowNodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string nodeId = Guid.NewGuid().ToString();
    
    [ObservableProperty]
    private FlowNodeType nodeType;
    
    [ObservableProperty]
    private string name = "Node";
    
    [ObservableProperty]
    private double x;
    
    [ObservableProperty]
    private double y;
    
    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private byte[]? previewImage;
}
