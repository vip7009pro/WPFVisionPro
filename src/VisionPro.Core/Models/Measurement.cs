using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Represents a single measurement value
/// </summary>
public class Measurement
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public double Value { get; set; }
    
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "mm";
    
    [JsonPropertyName("nominal")]
    public double Nominal { get; set; }
    
    [JsonPropertyName("tolerancePlus")]
    public double TolerancePlus { get; set; }
    
    [JsonPropertyName("toleranceMinus")]
    public double ToleranceMinus { get; set; }
    
    [JsonPropertyName("status")]
    public InspectionStatus Status { get; set; }
    
    /// <summary>
    /// Check if the measurement is within tolerance
    /// </summary>
    public bool IsWithinTolerance()
    {
        return Value >= (Nominal - ToleranceMinus) && Value <= (Nominal + TolerancePlus);
    }
}

/// <summary>
/// Represents a detected defect
/// </summary>
public class Defect
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }
    
    [JsonPropertyName("width")]
    public double Width { get; set; }
    
    [JsonPropertyName("height")]
    public double Height { get; set; }
    
    [JsonPropertyName("area")]
    public double Area { get; set; }
    
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Low";
}
