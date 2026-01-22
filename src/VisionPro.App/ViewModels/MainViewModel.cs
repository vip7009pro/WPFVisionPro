using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionPro.Core.Enums;

namespace VisionPro.App.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    
    [ObservableProperty]
    private int selectedTabIndex;
    
    [ObservableProperty]
    private string currentProductName = "No Product Loaded";
    
    [ObservableProperty]
    private DateTime currentTime = DateTime.Now;
    
    [ObservableProperty]
    private InspectionStatus lastResult = InspectionStatus.NotInspected;
    
    [ObservableProperty]
    private int totalCount;
    
    [ObservableProperty]
    private int okCount;
    
    [ObservableProperty]
    private int ngCount;
    
    [ObservableProperty]
    private long cycleTimeMs;
    
    [ObservableProperty]
    private bool isRunMode;
    
    [ObservableProperty]
    private bool isCameraConnected;
    
    [ObservableProperty]
    private bool isPlcConnected;
    
    public SolidColorBrush LastResultBrush => LastResult switch
    {
        InspectionStatus.OK => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
        InspectionStatus.NG => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
        _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))                  // Gray
    };
    
    public string LastResultText => LastResult switch
    {
        InspectionStatus.OK => "OK",
        InspectionStatus.NG => "NG",
        InspectionStatus.Error => "ERROR",
        _ => "--"
    };
    
    public double Yield => TotalCount > 0 ? (double)OkCount / TotalCount * 100 : 100;
    
    public SolidColorBrush CameraStatusBrush => IsCameraConnected 
        ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) 
        : new SolidColorBrush(Color.FromRgb(244, 67, 54));
    
    public SolidColorBrush PlcStatusBrush => IsPlcConnected
        ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
        : new SolidColorBrush(Color.FromRgb(255, 152, 0));
    
    public string ModeText => IsRunMode ? "RUN" : "SETUP";
    
    public SolidColorBrush ModeBrush => IsRunMode
        ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
        : new SolidColorBrush(Color.FromRgb(38, 166, 154));
    
    public MainViewModel()
    {
        _timer = new DispatcherTimer 
        { 
            Interval = TimeSpan.FromSeconds(1) 
        };
        _timer.Tick += (s, e) => CurrentTime = DateTime.Now;
        _timer.Start();
    }
    
    /// <summary>
    /// Update inspection result and statistics
    /// </summary>
    public void UpdateResult(InspectionStatus status, long cycleTime)
    {
        LastResult = status;
        CycleTimeMs = cycleTime;
        TotalCount++;
        
        if (status == InspectionStatus.OK)
            OkCount++;
        else if (status == InspectionStatus.NG)
            NgCount++;
        
        OnPropertyChanged(nameof(LastResultBrush));
        OnPropertyChanged(nameof(LastResultText));
        OnPropertyChanged(nameof(Yield));
    }
    
    /// <summary>
    /// Reset statistics
    /// </summary>
    [RelayCommand]
    private void ResetStatistics()
    {
        TotalCount = 0;
        OkCount = 0;
        NgCount = 0;
        LastResult = InspectionStatus.NotInspected;
        OnPropertyChanged(nameof(Yield));
        OnPropertyChanged(nameof(LastResultBrush));
        OnPropertyChanged(nameof(LastResultText));
    }
    
    /// <summary>
    /// Toggle between Run and Setup mode
    /// </summary>
    [RelayCommand]
    private void ToggleMode()
    {
        IsRunMode = !IsRunMode;
        OnPropertyChanged(nameof(ModeText));
        OnPropertyChanged(nameof(ModeBrush));
    }
    
    partial void OnIsCameraConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CameraStatusBrush));
    }
    
    partial void OnIsPlcConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(PlcStatusBrush));
    }
}
