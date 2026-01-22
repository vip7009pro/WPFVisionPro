using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VisionPro.App.ViewModels.Config;

public partial class InputImageConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedSourceType = "File"; // USB, IP, File, Folder

    public ObservableCollection<string> SourceTypes { get; } = new() { "USB Camera", "IP Camera", "File", "Folder" };

    [ObservableProperty]
    private string sourcePath = string.Empty;

    [ObservableProperty]
    private int cameraIndex = 0;

    [ObservableProperty]
    private string ipAddress = "192.168.1.100";

    [ObservableProperty]
    private double brightness = 0;

    [ObservableProperty]
    private double contrast = 1.0;

    [ObservableProperty]
    private double saturation = 1.0;

    [ObservableProperty]
    private bool autoExposure = true;

    public InputImageConfigViewModel()
    {
    }

    // This would eventually sync with the actual Node model
}
