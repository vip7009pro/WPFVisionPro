using System.Text.Json;
using OpenCvSharp;

namespace VisionPro.Vision.ImageSources;

/// <summary>
/// Image source for USB webcams
/// </summary>
public class UsbCameraSource : ImageSourceBase
{
    private int _cameraIndex;
    private readonly string _sourceId;
    
    public override string SourceId => _sourceId;
    public override string Name => $"USB Camera {_cameraIndex}";
    
    public UsbCameraSource(int cameraIndex = 0)
    {
        _cameraIndex = cameraIndex;
        _sourceId = $"usb_camera_{cameraIndex}";
    }
    
    public override async Task<bool> InitializeAsync(JsonElement? configuration = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (configuration.HasValue)
                {
                    var config = configuration.Value;
                    if (config.TryGetProperty("cameraIndex", out var indexProp))
                        _cameraIndex = indexProp.GetInt32();
                }
                
                _capture = new VideoCapture(_cameraIndex);
                
                if (!_capture.IsOpened())
                {
                    RaiseError(new Exception($"Failed to open camera at index {_cameraIndex}"));
                    return false;
                }
                
                // Apply configuration
                if (configuration.HasValue)
                {
                    var config = configuration.Value;
                    
                    if (config.TryGetProperty("width", out var widthProp))
                        _capture.Set(VideoCaptureProperties.FrameWidth, widthProp.GetInt32());
                    
                    if (config.TryGetProperty("height", out var heightProp))
                        _capture.Set(VideoCaptureProperties.FrameHeight, heightProp.GetInt32());
                    
                    if (config.TryGetProperty("frameRate", out var fpsProp))
                        _capture.Set(VideoCaptureProperties.Fps, fpsProp.GetInt32());
                    
                    if (config.TryGetProperty("exposure", out var expProp))
                        _capture.Set(VideoCaptureProperties.Exposure, expProp.GetDouble());
                    
                    if (config.TryGetProperty("gain", out var gainProp))
                        _capture.Set(VideoCaptureProperties.Gain, gainProp.GetDouble());
                }
                
                // Read actual values
                Width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
                Height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
                FrameRate = _capture.Get(VideoCaptureProperties.Fps);
                
                RaiseConnectionStatusChanged(true);
                return true;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return false;
            }
        }, cancellationToken);
    }
}
