using System.Windows.Controls;

namespace VisionPro.App.Views;

/// <summary>
/// Interaction logic for LiveViewControl.xaml
/// </summary>
public partial class LiveViewControl : UserControl
{
    public LiveViewControl()
    {
        InitializeComponent();
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.LiveViewViewModel>(App.Services);
        }
    }
}
