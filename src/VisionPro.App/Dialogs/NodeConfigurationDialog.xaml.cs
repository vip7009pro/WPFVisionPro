using System.Windows;
using VisionPro.App.ViewModels;

namespace VisionPro.App.Dialogs;

public partial class NodeConfigurationDialog : Window
{
    public NodeConfigurationDialog(FlowNodeViewModel node)
    {
        InitializeComponent();
        DataContext = node;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
