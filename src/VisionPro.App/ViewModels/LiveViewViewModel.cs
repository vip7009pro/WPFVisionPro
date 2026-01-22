using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;
using VisionPro.Vision.ImageSources;

namespace VisionPro.App.ViewModels;

/// <summary>
/// ViewModel for Live View screen
/// </summary>
public partial class LiveViewViewModel : ObservableObject, IDisposable
{
    private IImageSource? _imageSource;
    private CancellationTokenSource? _captureCts;
    
    [ObservableProperty]
    private BitmapSource? currentImage;
    
    [ObservableProperty]
    private bool isCapturing;
    
    [ObservableProperty]
    private string statusText = "Disconnected";
    
    [ObservableProperty]
    private double fps;
    
    [ObservableProperty]
    private int imageWidth;
    
    [ObservableProperty]
    private int imageHeight;
    
    [ObservableProperty]
    private double zoomLevel = 1.0;
    
    private DateTime _lastFrameTime = DateTime.Now;
    private int _frameCount;
    
    [RelayCommand]
    private async Task ConnectCameraAsync()
    {
        try
        {
            StatusText = "Connecting...";
            
            _imageSource = new UsbCameraSource(0);
            var success = await _imageSource.InitializeAsync();
            
            if (success)
            {
                StatusText = "Connected";
                ImageWidth = _imageSource.Width;
                ImageHeight = _imageSource.Height;
            }
            else
            {
                StatusText = "Connection failed";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task StartCaptureAsync()
    {
        if (_imageSource == null || IsCapturing)
            return;
        
        IsCapturing = true;
        _captureCts = new CancellationTokenSource();
        
        try
        {
            await _imageSource.StartContinuousCaptureAsync(OnFrameCaptured, _captureCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            StatusText = $"Capture error: {ex.Message}";
        }
        
        IsCapturing = false;
    }
    
    [RelayCommand]
    private async Task StopCaptureAsync()
    {
        _captureCts?.Cancel();
        
        if (_imageSource != null)
        {
            await _imageSource.StopContinuousCaptureAsync();
        }
        
        IsCapturing = false;
    }
    
    [RelayCommand]
    private async Task CaptureSnapshotAsync()
    {
        if (_imageSource == null)
            return;
        
        try
        {
            var frame = await _imageSource.CaptureAsync();
            if (frame != null)
            {
                UpdateImage(frame, _imageSource.Width, _imageSource.Height);
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Snapshot error: {ex.Message}";
        }
    }
    
    private void OnFrameCaptured(byte[] frameData)
    {
        if (_imageSource == null) return;
        
        // Calculate FPS
        _frameCount++;
        var now = DateTime.Now;
        var elapsed = (now - _lastFrameTime).TotalSeconds;
        if (elapsed >= 1.0)
        {
            Fps = _frameCount / elapsed;
            _frameCount = 0;
            _lastFrameTime = now;
        }
        
        UpdateImage(frameData, _imageSource.Width, _imageSource.Height);
    }
    
    private void UpdateImage(byte[] data, int width, int height)
    {
        // Update on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var bitmap = BitmapSource.Create(
                    width, height,
                    96, 96,
                    PixelFormats.Bgr24,
                    null,
                    data,
                    width * 3);
                bitmap.Freeze();
                
                CurrentImage = bitmap;
                ImageWidth = width;
                ImageHeight = height;
            }
            catch
            {
                // Ignore frame errors
            }
        });
    }
    
    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = Math.Min(5.0, ZoomLevel * 1.25);
    }
    
    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = Math.Max(0.1, ZoomLevel / 1.25);
    }
    
    [RelayCommand]
    private void ZoomFit()
    {
        ZoomLevel = 1.0;
    }
    
    public void Dispose()
    {
        _captureCts?.Cancel();
        _captureCts?.Dispose();
        _imageSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
