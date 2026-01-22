namespace VisionPro.Core.Enums;

/// <summary>
/// Represents the type of Region of Interest (ROI)
/// </summary>
public enum ROIType
{
    /// <summary>Rectangular ROI</summary>
    Rectangle = 0,
    
    /// <summary>Circular ROI</summary>
    Circle = 1,
    
    /// <summary>Triangular ROI</summary>
    Triangle = 2,
    
    /// <summary>Polygon ROI with arbitrary vertices</summary>
    Polygon = 3
}

/// <summary>
/// Represents the usage mode of an ROI
/// </summary>
public enum ROIUsage
{
    /// <summary>Active processing area - inspection occurs inside</summary>
    Include = 0,
    
    /// <summary>Mask area - inspection is excluded from this region</summary>
    Exclude = 1
}
