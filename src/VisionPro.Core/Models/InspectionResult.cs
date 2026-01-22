using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Complete result of an inspection cycle
/// </summary>
public class InspectionResult
{
    /// <summary>
    /// Unique identifier for this result
    /// </summary>
    [JsonPropertyName("resultId")]
    public string ResultId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Product ID that was inspected
    /// </summary>
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>
    /// Overall inspection status
    /// </summary>
    [JsonPropertyName("status")]
    public InspectionStatus Status { get; set; } = InspectionStatus.NotInspected;
    
    /// <summary>
    /// Timestamp when inspection started
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Duration of the inspection in milliseconds
    /// </summary>
    [JsonPropertyName("durationMs")]
    public long DurationMs { get; set; }
    
    /// <summary>
    /// All measurements taken during inspection
    /// </summary>
    [JsonPropertyName("measurements")]
    public List<Measurement> Measurements { get; set; } = new();
    
    /// <summary>
    /// All defects detected during inspection
    /// </summary>
    [JsonPropertyName("defects")]
    public List<Defect> Defects { get; set; } = new();
    
    /// <summary>
    /// Path to the captured image (if saved)
    /// </summary>
    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Path to the result overlay image (if saved)
    /// </summary>
    [JsonPropertyName("overlayImagePath")]
    public string? OverlayImagePath { get; set; }
    
    /// <summary>
    /// Product position found by pattern matching
    /// </summary>
    [JsonPropertyName("productPosition")]
    public ProductPosition? ProductPosition { get; set; }
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    /// <summary>
    /// Error message if inspection failed
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the detected position and orientation of a product
/// </summary>
public class ProductPosition
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
    
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }
    
    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
