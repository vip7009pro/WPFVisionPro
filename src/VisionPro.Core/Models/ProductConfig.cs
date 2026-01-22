using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Complete product configuration loaded from config files
/// </summary>
public class ProductConfig
{
    /// <summary>
    /// Basic product information
    /// </summary>
    [JsonPropertyName("product")]
    public ProductDefinition Product { get; set; } = new();
    
    /// <summary>
    /// Camera/image source configuration
    /// </summary>
    [JsonPropertyName("camera")]
    public CameraConfig Camera { get; set; } = new();
    
    /// <summary>
    /// Flow/pipeline definition
    /// </summary>
    [JsonPropertyName("flow")]
    public FlowDefinition Flow { get; set; } = new();
    
    /// <summary>
    /// ROI definitions
    /// </summary>
    [JsonPropertyName("rois")]
    public ROICollection ROIs { get; set; } = new();
    
    /// <summary>
    /// Threshold and measurement specifications
    /// </summary>
    [JsonPropertyName("thresholds")]
    public ThresholdConfig Thresholds { get; set; } = new();
    
    /// <summary>
    /// IO mapping configuration
    /// </summary>
    [JsonPropertyName("ioMapping")]
    public IOMapping IOMapping { get; set; } = new();
}

/// <summary>
/// Basic product definition
/// </summary>
public class ProductDefinition
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;
    
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Camera/image source configuration
/// </summary>
public class CameraConfig
{
    [JsonPropertyName("sourceType")]
    public ImageSourceType SourceType { get; set; } = ImageSourceType.UsbCamera;
    
    [JsonPropertyName("cameraIndex")]
    public int CameraIndex { get; set; }
    
    [JsonPropertyName("rtspUrl")]
    public string? RtspUrl { get; set; }
    
    [JsonPropertyName("videoPath")]
    public string? VideoPath { get; set; }
    
    [JsonPropertyName("imageFolderPath")]
    public string? ImageFolderPath { get; set; }
    
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;
    
    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;
    
    [JsonPropertyName("frameRate")]
    public int FrameRate { get; set; } = 30;
    
    [JsonPropertyName("exposure")]
    public double Exposure { get; set; } = -6;
    
    [JsonPropertyName("gain")]
    public double Gain { get; set; } = 1.0;
    
    [JsonPropertyName("whiteBalance")]
    public string WhiteBalance { get; set; } = "auto";
}

/// <summary>
/// Threshold and measurement specifications
/// </summary>
public class ThresholdConfig
{
    [JsonPropertyName("defectThresholds")]
    public List<DefectThreshold> DefectThresholds { get; set; } = new();
    
    [JsonPropertyName("measurementSpecs")]
    public List<MeasurementSpec> MeasurementSpecs { get; set; } = new();
}

/// <summary>
/// Defect detection threshold configuration
/// </summary>
public class DefectThreshold
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("binaryThreshold")]
    public int BinaryThreshold { get; set; } = 128;
    
    [JsonPropertyName("minArea")]
    public double MinArea { get; set; } = 10;
    
    [JsonPropertyName("maxArea")]
    public double MaxArea { get; set; } = 10000;
    
    [JsonPropertyName("morphologyKernel")]
    public int MorphologyKernel { get; set; } = 3;
    
    [JsonPropertyName("detectWhite")]
    public bool DetectWhite { get; set; } = true;
    
    [JsonPropertyName("detectBlack")]
    public bool DetectBlack { get; set; } = true;
}

/// <summary>
/// Measurement specification
/// </summary>
public class MeasurementSpec
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "distance";
    
    [JsonPropertyName("nominal")]
    public double Nominal { get; set; }
    
    [JsonPropertyName("tolerancePlus")]
    public double TolerancePlus { get; set; }
    
    [JsonPropertyName("toleranceMinus")]
    public double ToleranceMinus { get; set; }
    
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "mm";
    
    [JsonPropertyName("pixelsPerUnit")]
    public double PixelsPerUnit { get; set; } = 1.0;
}

/// <summary>
/// IO mapping for PLC communication
/// </summary>
public class IOMapping
{
    [JsonPropertyName("triggerInput")]
    public IOPoint? TriggerInput { get; set; }
    
    [JsonPropertyName("resultOutput")]
    public IOPoint? ResultOutput { get; set; }
    
    [JsonPropertyName("busyOutput")]
    public IOPoint? BusyOutput { get; set; }
    
    [JsonPropertyName("digitalInputs")]
    public List<IOPoint> DigitalInputs { get; set; } = new();
    
    [JsonPropertyName("digitalOutputs")]
    public List<IOPoint> DigitalOutputs { get; set; } = new();
}

/// <summary>
/// Single IO point definition
/// </summary>
public class IOPoint
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
    
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "bool";
}
