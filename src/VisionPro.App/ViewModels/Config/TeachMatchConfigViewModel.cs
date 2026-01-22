using CommunityToolkit.Mvvm.ComponentModel;
using VisionPro.App.ViewModels;

namespace VisionPro.App.ViewModels.Config;

public partial class TeachMatchConfigViewModel : ObservableObject
{
    [ObservableProperty]
    private double threshold = 0.7;

    [ObservableProperty]
    private int maxFeatures = 500;

    [ObservableProperty]
    private byte[]? templateImage;

    [ObservableProperty]
    private byte[]? previewImage;

    // The ROI Editor for selecting the match area
    [ObservableProperty]
    private ROIEditorViewModel roiEditor;

    public TeachMatchConfigViewModel(ROIEditorViewModel roiVm)
    {
        RoiEditor = roiVm;
    }
}
