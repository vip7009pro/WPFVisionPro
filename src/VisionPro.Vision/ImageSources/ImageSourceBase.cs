using OpenCvSharp;
using VisionPro.Core.Interfaces;

namespace VisionPro.Vision.ImageSources;

/// <summary>
/// Base class for all image sources providing common functionality
/// </summary>
public abstract class ImageSourceBase : IImageSource
{
    protected VideoCapture? _capture;
    protected bool _isConnected;
    protected bool _isContinuousCapture;
    protected CancellationTokenSource? _continuousCts;
    
    public abstract string SourceId { get; }
    public abstract string Name { get; }
    
    public bool IsConnected => _isConnected;
    public double FrameRate { get; protected set; }
    public int Width { get; protected set; }
    public int Height { get; protected set; }
    
    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;
    
    public abstract Task<bool> InitializeAsync(System.Text.Json.JsonElement? configuration = null, CancellationToken cancellationToken = default);
    
    public virtual async Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default)
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
                    Width = frame.Width;
                    Height = frame.Height;
                    
                    var data = new byte[frame.Total() * frame.ElemSize()];
                    System.Runtime.InteropServices.Marshal.Copy(frame.Data, data, 0, data.Length);
                    return data;
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return null;
        }, cancellationToken);
    }
    
    public virtual async Task StartContinuousCaptureAsync(Action<byte[]> onFrameCaptured, CancellationToken cancellationToken = default)
    {
        if (_isContinuousCapture) return;
        
        _isContinuousCapture = true;
        _continuousCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        await Task.Run(async () =>
        {
            while (!_continuousCts.Token.IsCancellationRequested && _isConnected)
            {
                var frame = await CaptureAsync(_continuousCts.Token);
                if (frame != null)
                {
                    onFrameCaptured(frame);
                }
                
                // Control frame rate
                await Task.Delay((int)(1000 / Math.Max(1, FrameRate)), _continuousCts.Token);
            }
        }, _continuousCts.Token);
    }
    
    public virtual Task StopContinuousCaptureAsync()
    {
        _isContinuousCapture = false;
        _continuousCts?.Cancel();
        _continuousCts?.Dispose();
        _continuousCts = null;
        return Task.CompletedTask;
    }
    
    protected void RaiseConnectionStatusChanged(bool isConnected)
    {
        _isConnected = isConnected;
        ConnectionStatusChanged?.Invoke(this, isConnected);
    }
    
    protected void RaiseError(Exception ex)
    {
        ErrorOccurred?.Invoke(this, ex);
    }
    
    public virtual void Dispose()
    {
        StopContinuousCaptureAsync().Wait();
        _capture?.Release();
        _capture?.Dispose();
        _capture = null;
        _isConnected = false;
        GC.SuppressFinalize(this);
    }
}
