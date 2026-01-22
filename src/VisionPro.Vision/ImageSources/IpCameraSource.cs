using System.Text.Json;
using OpenCvSharp;

namespace VisionPro.Vision.ImageSources;

/// <summary>
/// Image source for IP cameras (RTSP streams)
/// </summary>
public class IpCameraSource : ImageSourceBase
{
    private string _rtspUrl = string.Empty;
    private int _reconnectAttempts = 3;
    private int _reconnectDelayMs = 1000;
    private readonly string _sourceId;
    
    public override string SourceId => _sourceId;
    public override string Name => $"IP Camera ({_rtspUrl})";
    
    public IpCameraSource(string rtspUrl = "")
    {
        _rtspUrl = rtspUrl;
        _sourceId = $"ip_camera_{Guid.NewGuid():N}";
    }
    
    public override async Task<bool> InitializeAsync(JsonElement? configuration = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            try
            {
                if (configuration.HasValue)
                {
                    var config = configuration.Value;
                    
                    if (config.TryGetProperty("rtspUrl", out var urlProp))
                        _rtspUrl = urlProp.GetString() ?? string.Empty;
                    
                    if (config.TryGetProperty("reconnectAttempts", out var attemptsProp))
                        _reconnectAttempts = attemptsProp.GetInt32();
                    
                    if (config.TryGetProperty("reconnectDelayMs", out var delayProp))
                        _reconnectDelayMs = delayProp.GetInt32();
                }
                
                if (string.IsNullOrEmpty(_rtspUrl))
                {
                    RaiseError(new Exception("RTSP URL is required"));
                    return false;
                }
                
                // Try to connect with retries
                for (int attempt = 0; attempt < _reconnectAttempts; attempt++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;
                    
                    _capture = new VideoCapture(_rtspUrl, VideoCaptureAPIs.FFMPEG);
                    
                    if (_capture.IsOpened())
                    {
                        Width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
                        Height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
                        FrameRate = _capture.Get(VideoCaptureProperties.Fps);
                        
                        if (FrameRate <= 0) FrameRate = 25; // Default for RTSP
                        
                        RaiseConnectionStatusChanged(true);
                        return true;
                    }
                    
                    _capture.Dispose();
                    await Task.Delay(_reconnectDelayMs, cancellationToken);
                }
                
                RaiseError(new Exception($"Failed to connect to IP camera at {_rtspUrl} after {_reconnectAttempts} attempts"));
                return false;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return false;
            }
        }, cancellationToken);
    }
}
