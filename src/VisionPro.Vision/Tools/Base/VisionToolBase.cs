using System.Text.Json;
using VisionPro.Core.Enums;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;

namespace VisionPro.Vision.Tools.Base;

/// <summary>
/// Base class for all vision tools providing common functionality
/// </summary>
public abstract class VisionToolBase : IVisionTool
{
    protected JsonElement _configuration;
    protected bool _isConfigured;
    
    public string ToolId { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Vision Tool";
    public abstract VisionToolType ToolType { get; }
    public bool IsConfigured => _isConfigured;
    
    public virtual void Configure(JsonElement configuration)
    {
        _configuration = configuration;
        OnConfigure(configuration);
        _isConfigured = true;
    }
    
    /// <summary>
    /// Override to handle configuration
    /// </summary>
    protected virtual void OnConfigure(JsonElement configuration) { }
    
    public abstract Task<VisionToolResult> ExecuteAsync(
        byte[] imageData,
        int width,
        int height,
        ROI? roi = null,
        CancellationToken cancellationToken = default);
    
    public virtual JsonElement GetConfiguration()
    {
        return _configuration;
    }
    
    public virtual void Reset()
    {
        _isConfigured = false;
    }
    
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
