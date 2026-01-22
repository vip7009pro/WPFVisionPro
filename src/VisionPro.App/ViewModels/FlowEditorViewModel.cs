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
    private ObservableCollection<FlowConnectionViewModel> connections = new();
    
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
    private void AddNode(string nodeTypeString)
    {
        if (Enum.TryParse<FlowNodeType>(nodeTypeString, out var nodeType))
        {
            var node = new FlowNodeViewModel
            {
                NodeType = nodeType,
                Name = GetDefaultNodeName(nodeType),
                X = 100 + Nodes.Count * 50,
                Y = 100 + Nodes.Count * 30
            };
            
            Nodes.Add(node);
            InitializeDefaultPorts(node);
            SelectedNode = node;
            IsModified = true;
        }
    }

    private void InitializeDefaultPorts(FlowNodeViewModel node)
    {
        switch (node.NodeType)
        {
            case FlowNodeType.InputImage:
                node.OutputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Image Out", VisionPro.Flow.Nodes.Ports.PortType.Image, VisionPro.Flow.Nodes.Ports.PortDirection.Output, node.NodeId));
                break;
            case FlowNodeType.TeachMatch:
                node.InputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Image In", VisionPro.Flow.Nodes.Ports.PortType.Image, VisionPro.Flow.Nodes.Ports.PortDirection.Input, node.NodeId));
                node.OutputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Image Out", VisionPro.Flow.Nodes.Ports.PortType.Image, VisionPro.Flow.Nodes.Ports.PortDirection.Output, node.NodeId));
                node.OutputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Position", VisionPro.Flow.Nodes.Ports.PortType.Coordinates, VisionPro.Flow.Nodes.Ports.PortDirection.Output, node.NodeId));
                break;
            case FlowNodeType.Measurement:
            case FlowNodeType.DefectDetection:
            case FlowNodeType.ROIApply:
                node.InputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Image In", VisionPro.Flow.Nodes.Ports.PortType.Image, VisionPro.Flow.Nodes.Ports.PortDirection.Input, node.NodeId));
                node.OutputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Image Out", VisionPro.Flow.Nodes.Ports.PortType.Image, VisionPro.Flow.Nodes.Ports.PortDirection.Output, node.NodeId));
                node.OutputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Data", VisionPro.Flow.Nodes.Ports.PortType.Data, VisionPro.Flow.Nodes.Ports.PortDirection.Output, node.NodeId));
                break;
            case FlowNodeType.FinalDecision:
                node.InputPorts.Add(new VisionPro.Flow.Nodes.Ports.NodePort("Data In", VisionPro.Flow.Nodes.Ports.PortType.Data, VisionPro.Flow.Nodes.Ports.PortDirection.Input, node.NodeId));
                break;
        }
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
    private async Task SaveFlow()
    {
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "VisionPro Flow (*.json)|*.json",
            FileName = FlowName
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var serializer = new VisionPro.Flow.Serialization.FlowSerializer();
                
                // Convert ViewModels to FlowDefinition
                // Note: Ideally we should use the FlowEngine to get the definition, but 
                // since the ViewModel represents the "design" state which might differ from the "running" state in the engine
                // we'll construct it here or push to engine then save.
                // For simplicity, let's construct it here to match what's on screen.
                
                var definition = new FlowDefinition
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
                    Nodes = Nodes.Select(n => new FlowNodeDefinition
                    {
                        NodeId = n.NodeId,
                        NodeType = n.NodeType,
                        Name = n.Name,
                        PositionX = n.X,
                        PositionY = n.Y,
                        // Connections not fully implemented in VM yet, would go here
                        // For now we rely on the implementation plan's simplification
                    }).ToList()
                    // Connections would need to be captured from the UI or ViewModel state
                };
                
                await serializer.SaveAsync(definition, saveFileDialog.FileName);
                
                FlowName = definition.Name;
                IsModified = false;
                
                // Also update the engine
                _flowEngine.LoadFlow(definition);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving flow: {ex.Message}", "Error");
            }
        }
    }
    
    [RelayCommand]
    private async Task LoadFlow()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "VisionPro Flow (*.json)|*.json"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var serializer = new VisionPro.Flow.Serialization.FlowSerializer();
                var definition = await serializer.LoadAsync(openFileDialog.FileName);
                
                if (definition != null)
                {
                    Nodes.Clear();
                    FlowName = definition.Name;
                    
                    foreach (var nodeDef in definition.Nodes)
                    {
                        Nodes.Add(new FlowNodeViewModel
                        {
                            NodeId = nodeDef.NodeId,
                            NodeType = nodeDef.NodeType,
                            Name = nodeDef.Name,
                            X = nodeDef.PositionX,
                            Y = nodeDef.PositionY
                        });
                    }
                    
                    // Update engine
                    _flowEngine.LoadFlow(definition);
                    IsModified = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading flow: {ex.Message}", "Error");
            }
        }
    }
    
    public void ConnectNodes(FlowNodeViewModel source, VisionPro.Flow.Nodes.Ports.NodePort sourcePort, FlowNodeViewModel target, VisionPro.Flow.Nodes.Ports.NodePort targetPort)
    {
        // Prevent self connection
        if (source == target) return;
        
        // Prevent duplicate connection
        if (Connections.Any(c => c.Model.SourceNodeId == source.NodeId && c.Model.TargetNodeId == target.NodeId 
                                && c.Model.SourcePort == sourcePort.Name && c.Model.TargetPort == targetPort.Name))
            return;
            
        var connectionModel = new VisionPro.Core.Models.FlowConnection
        {
            SourceNodeId = source.NodeId,
            SourcePort = sourcePort.Name,
            TargetNodeId = target.NodeId,
            TargetPort = targetPort.Name
        };
        
        var connectionVm = new FlowConnectionViewModel(connectionModel, source, target);
        Connections.Add(connectionVm);
        
        // Mark ports as connected (visual only for now)
        sourcePort.IsConnected = true;
        targetPort.IsConnected = true;
        
        IsModified = true;
    }

    [RelayCommand]
    private void OpenConfiguration(FlowNodeViewModel node)
    {
        switch (node.NodeType)
        {
            case FlowNodeType.InputImage:
                var inputVm = new ViewModels.Config.InputImageConfigViewModel();
                node.ConfigurationContent = new Views.Config.InputImageConfigControl { DataContext = inputVm };
                break;

            case FlowNodeType.ConditionalBranch:
                var condVm = new ViewModels.Config.ConditionConfigViewModel();
                node.ConfigurationContent = new Views.Config.ConditionConfigControl { DataContext = condVm };
                break;

            case FlowNodeType.TeachMatch:
                var roiVm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.ROIEditorViewModel>(App.Services);
                var teachVm = new ViewModels.Config.TeachMatchConfigViewModel(roiVm);
                node.ConfigurationContent = new Views.Config.TeachMatchConfigControl { DataContext = teachVm };
                break;

            case FlowNodeType.FinalDecision:
                var finalVm = new ViewModels.Config.FinalDecisionConfigViewModel();
                node.ConfigurationContent = new Views.Config.FinalDecisionConfigControl { DataContext = finalVm };
                break;

            default:
                node.ConfigurationContent = new System.Windows.Controls.TextBlock 
                { 
                    Text = $"Configuration for {node.NodeType} not implemented.",
                    Foreground = System.Windows.Media.Brushes.White,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                break;
        }

        var dialog = new Dialogs.NodeConfigurationDialog(node);
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            IsModified = true;
        }
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

    [ObservableProperty]
    private ObservableCollection<VisionPro.Flow.Nodes.Ports.NodePort> inputPorts = new();

    [ObservableProperty]
    private ObservableCollection<VisionPro.Flow.Nodes.Ports.NodePort> outputPorts = new();

    [ObservableProperty]
    private object? configurationContent;
}
