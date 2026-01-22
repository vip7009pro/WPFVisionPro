using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VisionPro.App.ViewModels.Config;

public partial class FinalDecisionConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private string logicType = "AND"; // AND, OR

    public ObservableCollection<string> LogicTypes { get; } = new() { "AND", "OR" };

    [ObservableProperty]
    private bool showOverlay = true;

    [ObservableProperty]
    private double overlayOpacity = 0.5;

    [ObservableProperty]
    private byte[]? resultImage;

    [ObservableProperty]
    private string inspectionStatus = "OK"; // OK, NG

    public FinalDecisionConfigViewModel()
    {
    }
}
