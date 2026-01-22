namespace VisionPro.Core.Enums;

/// <summary>
/// Represents the result status of an inspection
/// </summary>
public enum InspectionStatus
{
    /// <summary>Inspection not yet performed</summary>
    NotInspected = 0,
    
    /// <summary>Inspection passed all criteria</summary>
    OK = 1,
    
    /// <summary>Inspection failed one or more criteria</summary>
    NG = 2,
    
    /// <summary>Inspection encountered an error</summary>
    Error = 3,
    
    /// <summary>Inspection was skipped</summary>
    Skipped = 4
}
