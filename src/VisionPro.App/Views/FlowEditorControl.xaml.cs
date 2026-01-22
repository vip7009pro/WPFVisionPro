using System.Windows.Controls;

namespace VisionPro.App.Views;

public partial class FlowEditorControl : UserControl
{
    public FlowEditorControl()
    {
        InitializeComponent();
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.FlowEditorViewModel>(App.Services);
        }
    }

    private bool _isDragging;
    private System.Windows.Point _lastMousePosition;
    private ViewModels.FlowNodeViewModel? _draggedNode;

    private void Node_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement element && element.DataContext is ViewModels.FlowNodeViewModel node)
        {
            _isDragging = true;
            _draggedNode = node;
            _lastMousePosition = e.GetPosition(this);
            element.CaptureMouse();
            
            // Also select the node in ViewModel
            if (DataContext is ViewModels.FlowEditorViewModel vm)
            {
                vm.SelectedNode = node;
                
                // Double Click to Configure
                if (e.ClickCount == 2)
                {
                   vm.OpenConfigurationCommand.Execute(node);
                }
            }
        }
    }

    private void Node_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _draggedNode = null;
            (sender as System.Windows.FrameworkElement)?.ReleaseMouseCapture();
        }
    }

    private void Node_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging && _draggedNode != null)
        {
            var currentPosition = e.GetPosition(this);
            var offset = currentPosition - _lastMousePosition;
            
            _draggedNode.X += offset.X;
            _draggedNode.Y += offset.Y;
            
            _lastMousePosition = currentPosition;
        }
    }
    private bool _isConnecting;
    private ViewModels.FlowNodeViewModel? _sourceNode;
    private VisionPro.Flow.Nodes.Ports.NodePort? _sourcePort;
    private System.Windows.Shapes.Ellipse? _sourcePortElement;

    private void Port_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Shapes.Ellipse ellipse && ellipse.Tag is VisionPro.Flow.Nodes.Ports.NodePort port)
        {
            // Only allow dragging from Output ports (Right side)
            if (port.Direction != VisionPro.Flow.Nodes.Ports.PortDirection.Output) return;

            // Find parent NodeViewModel
            var element = ellipse as System.Windows.FrameworkElement;
            while (element != null && !(element.DataContext is ViewModels.FlowNodeViewModel))
            {
                element = System.Windows.Media.VisualTreeHelper.GetParent(element) as System.Windows.FrameworkElement;
            }

            if (element?.DataContext is ViewModels.FlowNodeViewModel node)
            {
                _isConnecting = true;
                _sourceNode = node;
                _sourcePort = port;
                _sourcePortElement = ellipse;
                ellipse.CaptureMouse();
                
                // Show Temp Path
                TempConnectionPath.Visibility = System.Windows.Visibility.Visible;
                var startPoint = ellipse.TranslatePoint(new System.Windows.Point(ellipse.ActualWidth / 2, ellipse.ActualHeight / 2), this);
                TempConnectionFigure.StartPoint = startPoint;
                TempConnectionBezier.Point1 = startPoint; // Control Point 1
                TempConnectionBezier.Point2 = startPoint; // Control Point 2
                TempConnectionBezier.Point3 = startPoint; // End Point
                
                e.Handled = true;
            }
        }
    }

    private void Port_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isConnecting)
        {
            var currentPos = e.GetPosition(this);
            var startPos = TempConnectionFigure.StartPoint;
            
            // Update Bezier
            TempConnectionBezier.Point3 = currentPos;
            
            double dist = Math.Abs(currentPos.X - startPos.X) / 2;
            if (dist < 50) dist = 50;
            
            TempConnectionBezier.Point1 = new System.Windows.Point(startPos.X + dist, startPos.Y);
            TempConnectionBezier.Point2 = new System.Windows.Point(currentPos.X - dist, currentPos.Y);
            
            e.Handled = true;
        }
    }

    private void Port_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isConnecting)
        {
            _sourcePortElement?.ReleaseMouseCapture();
            TempConnectionPath.Visibility = System.Windows.Visibility.Collapsed;
            _isConnecting = false;

            // Check if dropped on a valid Input port
            if (sender is System.Windows.Shapes.Ellipse ellipse && ellipse.Tag is VisionPro.Flow.Nodes.Ports.NodePort targetPort)
            {
                if (targetPort.Direction == VisionPro.Flow.Nodes.Ports.PortDirection.Input)
                {
                   // Find target Node data context
                    var element = ellipse as System.Windows.FrameworkElement;
                    while (element != null && !(element.DataContext is ViewModels.FlowNodeViewModel))
                    {
                        element = System.Windows.Media.VisualTreeHelper.GetParent(element) as System.Windows.FrameworkElement;
                    }
                    
                    if (element?.DataContext is ViewModels.FlowNodeViewModel targetNode && DataContext is ViewModels.FlowEditorViewModel vm)
                    {
                        if (_sourceNode != targetNode)
                        {
                            vm.ConnectNodes(_sourceNode!, _sourcePort!, targetNode, targetPort);
                        }
                    }
                }
            }
            
            _sourceNode = null;
            _sourcePort = null;
            _sourcePortElement = null;
            e.Handled = true;
        }
    }

    private void Port_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // Optional highlight
        if (sender is System.Windows.Shapes.Ellipse ellipse)
        {
            ellipse.StrokeThickness = 2;
        }
    }

    private void Port_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is System.Windows.Shapes.Ellipse ellipse)
        {
            ellipse.StrokeThickness = 1;
        }
    }
}