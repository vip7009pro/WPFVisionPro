using System.Windows.Controls;

namespace VisionPro.App.Views;

public partial class ProductManagerControl : UserControl
{
    public ProductManagerControl()
    {
        InitializeComponent();
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ViewModels.ProductManagerViewModel>(App.Services);
        }
    }
}
