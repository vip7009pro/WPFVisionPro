using System.Text.Json.Serialization;
using VisionPro.Core.Enums;

namespace VisionPro.Core.Models;

/// <summary>
/// Result from a vision tool execution
/// </summary>
public class VisionToolResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("status")]
    public InspectionStatus Status { get; set; } = InspectionStatus.NotInspected;
    
    [JsonPropertyName("measurements")]
    public List<Measurement> Measurements { get; set; } = new();
    
    [JsonPropertyName("defects")]
    public List<Defect> Defects { get; set; } = new();
    
    [JsonPropertyName("position")]
    public ProductPosition? Position { get; set; }
    
    [JsonPropertyName("executionTimeMs")]
    public long ExecutionTimeMs { get; set; }
    
    [JsonPropertyName("outputImage")]
    public byte[]? OutputImage { get; set; }
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = new();
    
    public static VisionToolResult CreateSuccess() => new() { Success = true, Status = InspectionStatus.OK };
    public static VisionToolResult CreateFailure(string error) => new() { Success = false, Status = InspectionStatus.Error, ErrorMessage = error };
}

/// <summary>
/// Result from a flow node execution
/// </summary>
public class FlowNodeResult
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("status")]
    public InspectionStatus Status { get; set; } = InspectionStatus.NotInspected;
    
    [JsonPropertyName("outputImage")]
    public byte[]? OutputImage { get; set; }
    
    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = new();
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    [JsonPropertyName("executionTimeMs")]
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// Next node ID to execute (for conditional branching)
    /// </summary>
    [JsonPropertyName("nextNodeId")]
    public string? NextNodeId { get; set; }
    
    public static FlowNodeResult CreateSuccess(string nodeId) => new() { NodeId = nodeId, Success = true, Status = InspectionStatus.OK };
    public static FlowNodeResult CreateFailure(string nodeId, string error) => new() { NodeId = nodeId, Success = false, Status = InspectionStatus.Error, ErrorMessage = error };
}

/// <summary>
/// Validation result with errors
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Valid() => new() { IsValid = true };
    
    public static ValidationResult Invalid(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

/// <summary>
/// Context for flow execution - shared state between nodes
/// </summary>
public class FlowExecutionContext
{
    /// <summary>
    /// Current input image
    /// </summary>
    public byte[]? CurrentImage { get; set; }
    
    /// <summary>
    /// Image width
    /// </summary>
    public int ImageWidth { get; set; }
    
    /// <summary>
    /// Image height
    /// </summary>
    public int ImageHeight { get; set; }
    
    /// <summary>
    /// Detected product position (from teaching)
    /// </summary>
    public ProductPosition? ProductPosition { get; set; }
    
    /// <summary>
    /// All measurements collected during flow
    /// </summary>
    public List<Measurement> Measurements { get; set; } = new();
    
    /// <summary>
    /// All defects detected during flow
    /// </summary>
    public List<Defect> Defects { get; set; } = new();
    
    /// <summary>
    /// Results from each executed node
    /// </summary>
    public Dictionary<string, FlowNodeResult> NodeResults { get; set; } = new();
    
    /// <summary>
    /// Shared variables for inter-node communication
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();
    
    /// <summary>
    /// Current ROI collection
    /// </summary>
    public ROICollection? ROIs { get; set; }
    
    /// <summary>
    /// Threshold configuration
    /// </summary>
    public ThresholdConfig? Thresholds { get; set; }
    
    /// <summary>
    /// Cancellation token for the current execution
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    
    /// <summary>
    /// Flow start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;
}
