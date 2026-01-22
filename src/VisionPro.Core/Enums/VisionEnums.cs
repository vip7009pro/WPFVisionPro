namespace VisionPro.Core.Enums;

/// <summary>
/// Types of image sources supported by the system
/// </summary>
public enum ImageSourceType
{
    /// <summary>USB connected camera</summary>
    UsbCamera = 0,
    
    /// <summary>IP camera with RTSP stream</summary>
    IpCamera = 1,
    
    /// <summary>Video file playback</summary>
    VideoFile = 2,
    
    /// <summary>Image folder for batch processing</summary>
    ImageFolder = 3,
    
    /// <summary>Offline simulation mode</summary>
    Simulation = 4
}

/// <summary>
/// Types of vision tools available in the system
/// </summary>
public enum VisionToolType
{
    /// <summary>Template/Pattern matching tool</summary>
    TeachMatch = 0,
    
    /// <summary>Distance measurement tool</summary>
    DistanceMeasure = 1,
    
    /// <summary>Angle measurement tool</summary>
    AngleMeasure = 2,
    
    /// <summary>Defect detection tool</summary>
    DefectDetection = 3,
    
    /// <summary>Binary threshold tool</summary>
    Threshold = 4,
    
    /// <summary>Edge detection tool</summary>
    EdgeDetection = 5,
    
    /// <summary>Blob analysis tool</summary>
    BlobAnalysis = 6
}

/// <summary>
/// Types of flow nodes
/// </summary>
public enum FlowNodeType
{
    /// <summary>Input image source node</summary>
    InputImage = 0,
    
    /// <summary>Template matching node</summary>
    TeachMatch = 1,
    
    /// <summary>Apply ROI to image</summary>
    ROIApply = 2,
    
    /// <summary>Measurement node</summary>
    Measurement = 3,
    
    /// <summary>Threshold comparison node</summary>
    ThresholdCompare = 4,
    
    /// <summary>Conditional branching node</summary>
    ConditionalBranch = 5,
    
    /// <summary>Final decision node (OK/NG)</summary>
    FinalDecision = 6,
    
    /// <summary>Defect detection node</summary>
    DefectDetection = 7
}
