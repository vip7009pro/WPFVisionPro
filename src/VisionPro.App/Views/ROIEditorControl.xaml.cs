using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using VisionPro.App.ViewModels;

namespace VisionPro.App.Views;

public partial class ROIEditorControl : UserControl
{
    public ROIEditorControl()
    {
        InitializeComponent();
        
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            DataContext = App.Services.GetRequiredService<ROIEditorViewModel>();
        }
    }
}
