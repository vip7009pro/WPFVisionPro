using System.Text.Json;
using OpenCvSharp;

namespace VisionPro.Vision.ImageSources;

/// <summary>
/// Image source for video file playback
/// </summary>
public class VideoFileSource : ImageSourceBase
{
    private string _videoPath = string.Empty;
    private bool _loop = true;
    private readonly string _sourceId;
    
    public override string SourceId => _sourceId;
    public override string Name => $"Video: {Path.GetFileName(_videoPath)}";
    
    /// <summary>
    /// Total number of frames in the video
    /// </summary>
    public int TotalFrames { get; private set; }
    
    /// <summary>
    /// Current frame position
    /// </summary>
    public int CurrentFrame { get; private set; }
    
    public VideoFileSource(string videoPath = "")
    {
        _videoPath = videoPath;
        _sourceId = $"video_file_{Guid.NewGuid():N}";
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
                    
                    if (config.TryGetProperty("videoPath", out var pathProp))
                        _videoPath = pathProp.GetString() ?? string.Empty;
                    
                    if (config.TryGetProperty("loop", out var loopProp))
                        _loop = loopProp.GetBoolean();
                }
                
                if (string.IsNullOrEmpty(_videoPath) || !File.Exists(_videoPath))
                {
                    RaiseError(new Exception($"Video file not found: {_videoPath}"));
                    return false;
                }
                
                _capture = new VideoCapture(_videoPath);
                
                if (!_capture.IsOpened())
                {
                    RaiseError(new Exception($"Failed to open video file: {_videoPath}"));
                    return false;
                }
                
                Width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
                Height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
                FrameRate = _capture.Get(VideoCaptureProperties.Fps);
                TotalFrames = (int)_capture.Get(VideoCaptureProperties.FrameCount);
                
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
    
    public override async Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        if (_capture == null || !_isConnected)
            return null;
            
        return await Task.Run(() =>
        {
            try
            {
                using var frame = new Mat();
                if (_capture.Read(frame) && !frame.Empty())
                {
                    CurrentFrame = (int)_capture.Get(VideoCaptureProperties.PosFrames);
                    
                    var data = new byte[frame.Total() * frame.ElemSize()];
                    System.Runtime.InteropServices.Marshal.Copy(frame.Data, data, 0, data.Length);
                    return data;
                }
                else if (_loop)
                {
                    // Loop back to start
                    _capture.Set(VideoCaptureProperties.PosFrames, 0);
                    CurrentFrame = 0;
                    return CaptureAsync(cancellationToken).Result;
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return null;
        }, cancellationToken);
    }
    
    /// <summary>
    /// Seek to a specific frame
    /// </summary>
    public void SeekToFrame(int frameNumber)
    {
        if (_capture != null && frameNumber >= 0 && frameNumber < TotalFrames)
        {
            _capture.Set(VideoCaptureProperties.PosFrames, frameNumber);
            CurrentFrame = frameNumber;
        }
    }
}
