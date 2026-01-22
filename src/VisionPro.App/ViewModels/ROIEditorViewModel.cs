using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;

namespace VisionPro.App.ViewModels;

public partial class ROIEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ROI> rois = new();
    
    [ObservableProperty]
    private ROI? selectedROI;
    
    [ObservableProperty]
    private byte[]? backgroundImage;
    
    [ObservableProperty]
    private double zoomLevel = 1.0;
    
    [ObservableProperty]
    private bool isDrawing;
    
    [ObservableProperty]
    private ROIType activeTool = ROIType.Rectangle;
    
    public ROIEditorViewModel()
    {
        // Sample data for testing
        Rois.Add(new ROI 
        { 
            Name = "Search Area", 
            Type = ROIType.Rectangle, 
            Points = new double[] { 100, 100, 200, 150 }, // x, y, w, h
            Color = "#00FF00"
        });
    }
    
    [RelayCommand]
    private void AddROI(ROIType type)
    {
        var newRoi = new ROI
        {
            Name = $"New {type}",
            Type = type,
            Points = GetDefaultPoints(type),
            Color = GetNextColor()
        };
        
        Rois.Add(newRoi);
        SelectedROI = newRoi;
    }
    
    [RelayCommand]
    private void RemoveSelectedROI()
    {
        if (SelectedROI != null)
        {
            Rois.Remove(SelectedROI);
            SelectedROI = null;
        }
    }
    
    [RelayCommand]
    private void SetActiveTool(ROIType type)
    {
        ActiveTool = type;
        IsDrawing = true;
    }
    
    private double[] GetDefaultPoints(ROIType type)
    {
        // Center of a 640x480 view roughly
        return type switch
        {
            ROIType.Rectangle => new double[] { 200, 200, 100, 100 },
            ROIType.Circle => new double[] { 320, 240, 50 },
            ROIType.Polygon => new double[] { 200, 200, 300, 200, 250, 300 },
            _ => Array.Empty<double>()
        };
    }
    
    private string GetNextColor()
    {
        var colors = new[] { "#00FF00", "#FF0000", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF" };
        return colors[Rois.Count % colors.Length];
    }
}
