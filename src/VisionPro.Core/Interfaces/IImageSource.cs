namespace VisionPro.Core.Interfaces;

/// <summary>
/// Abstract interface for all image input sources.
/// Implementations include USB cameras, IP cameras, video files, and image folders.
/// </summary>
public interface IImageSource : IDisposable
{
    /// <summary>
    /// Unique identifier for this image source
    /// </summary>
    string SourceId { get; }
    
    /// <summary>
    /// Display name of the image source
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Indicates whether the source is currently connected and ready
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Current frame rate (frames per second)
    /// </summary>
    double FrameRate { get; }
    
    /// <summary>
    /// Image width in pixels
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// Image height in pixels
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// Initialize the image source with optional configuration
    /// </summary>
    /// <param name="configuration">JSON configuration element</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> InitializeAsync(System.Text.Json.JsonElement? configuration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Capture a single frame from the source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw image data as byte array (BGR format)</returns>
    Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start continuous capture mode
    /// </summary>
    /// <param name="onFrameCaptured">Callback invoked when a frame is captured</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartContinuousCaptureAsync(Action<byte[]> onFrameCaptured, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stop continuous capture mode
    /// </summary>
    Task StopContinuousCaptureAsync();
    
    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;
    
    /// <summary>
    /// Event raised when an error occurs
    /// </summary>
    event EventHandler<Exception>? ErrorOccurred;
}
