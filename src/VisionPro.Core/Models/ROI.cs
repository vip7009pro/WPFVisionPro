using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Represents a Region of Interest (ROI) for image processing
/// </summary>
public class ROI
{
    /// <summary>
    /// Unique identifier for this ROI
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name for the ROI
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of ROI shape
    /// </summary>
    [JsonPropertyName("type")]
    public ROIType Type { get; set; } = ROIType.Rectangle;
    
    /// <summary>
    /// Usage mode (include/exclude)
    /// </summary>
    [JsonPropertyName("usage")]
    public ROIUsage Usage { get; set; } = ROIUsage.Include;
    
    /// <summary>
    /// Points defining the ROI shape (format depends on type)
    /// Rectangle: [x, y, width, height]
    /// Circle: [centerX, centerY, radius]
    /// Triangle/Polygon: [x1, y1, x2, y2, ...]
    /// </summary>
    [JsonPropertyName("points")]
    public double[] Points { get; set; } = Array.Empty<double>();
    
    /// <summary>
    /// Rotation angle in degrees (for rectangle)
    /// </summary>
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }
    
    /// <summary>
    /// Whether the ROI is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Color for display (hex format)
    /// </summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#00FF00";
}

/// <summary>
/// Collection of ROIs for a product
/// </summary>
public class ROICollection
{
    [JsonPropertyName("rois")]
    public List<ROI> ROIs { get; set; } = new();
}
