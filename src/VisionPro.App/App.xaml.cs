using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using VisionPro.Flow.Engine;

namespace VisionPro.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
        
        // Set render quality
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
        
        // Show main window
        var mainWindow = new MainWindow();
        mainWindow.DataContext = Services.GetRequiredService<ViewModels.MainViewModel>();
        mainWindow.Show();
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Register ViewModels
        services.AddSingleton<ViewModels.MainViewModel>();
        services.AddTransient<ViewModels.LiveViewViewModel>();
        services.AddTransient<ViewModels.FlowEditorViewModel>();
        services.AddTransient<ViewModels.ROIEditorViewModel>();
        services.AddTransient<ViewModels.ProductManagerViewModel>();
        
        // Register Services
        services.AddSingleton<FlowEngine>();
        services.AddSingleton<Services.INavigationService, Services.NavigationService>();
    }
}
