using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;

namespace VisionPro.Core.Interfaces;

/// <summary>
/// Base interface for all vision processing tools.
/// Each tool performs a specific image analysis task.
/// </summary>
public interface IVisionTool : IDisposable
{
    /// <summary>
    /// Unique identifier for this tool instance
    /// </summary>
    string ToolId { get; }
    
    /// <summary>
    /// Display name of the tool
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Type of vision tool
    /// </summary>
    VisionToolType ToolType { get; }
    
    /// <summary>
    /// Indicates whether the tool is properly configured and ready to execute
    /// </summary>
    bool IsConfigured { get; }
    
    /// <summary>
    /// Configure the tool with JSON settings
    /// </summary>
    /// <param name="configuration">JSON configuration</param>
    void Configure(JsonElement configuration);
    
    /// <summary>
    /// Execute the vision tool on the provided image data
    /// </summary>
    /// <param name="imageData">Input image as byte array (BGR format)</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <param name="roi">Optional Region of Interest to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vision tool result</returns>
    Task<VisionToolResult> ExecuteAsync(
        byte[] imageData, 
        int width, 
        int height,
        ROI? roi = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the current configuration as JSON
    /// </summary>
    JsonElement GetConfiguration();
    
    /// <summary>
    /// Reset the tool to its default state
    /// </summary>
    void Reset();
}
