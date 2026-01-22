using System.Text.Json;
using OpenCvSharp;
using VisionPro.Core.Interfaces;

namespace VisionPro.Vision.ImageSources;

/// <summary>
/// Image source that loads images from a folder (batch processing / simulation)
/// </summary>
public class ImageFolderSource : IImageSource
{
    private readonly string _sourceId;
    private string _folderPath = string.Empty;
    private string[] _imageFiles = Array.Empty<string>();
    private int _currentIndex;
    private bool _loop = true;
    private bool _isConnected;
    private CancellationTokenSource? _continuousCts;
    
    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif" };
    
    public string SourceId => _sourceId;
    public string Name => $"Folder: {Path.GetFileName(_folderPath)}";
    public bool IsConnected => _isConnected;
    public double FrameRate { get; private set; } = 10;
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    /// <summary>
    /// Total number of images in the folder
    /// </summary>
    public int TotalImages => _imageFiles.Length;
    
    /// <summary>
    /// Current image index
    /// </summary>
    public int CurrentIndex => _currentIndex;
    
    /// <summary>
    /// Current image file path
    /// </summary>
    public string? CurrentImagePath => _currentIndex < _imageFiles.Length ? _imageFiles[_currentIndex] : null;
    
    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;
    
    public ImageFolderSource(string folderPath = "")
    {
        _folderPath = folderPath;
        _sourceId = $"image_folder_{Guid.NewGuid():N}";
    }
    
    public async Task<bool> InitializeAsync(JsonElement? configuration = null, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (configuration.HasValue)
                {
                    var config = configuration.Value;
                    
                    if (config.TryGetProperty("imageFolderPath", out var pathProp))
                        _folderPath = pathProp.GetString() ?? string.Empty;
                    
                    if (config.TryGetProperty("loop", out var loopProp))
                        _loop = loopProp.GetBoolean();
                    
                    if (config.TryGetProperty("frameRate", out var fpsProp))
                        FrameRate = fpsProp.GetDouble();
                }
                
                if (string.IsNullOrEmpty(_folderPath) || !Directory.Exists(_folderPath))
                {
                    ErrorOccurred?.Invoke(this, new Exception($"Image folder not found: {_folderPath}"));
                    return false;
                }
                
                // Find all image files
                _imageFiles = Directory.GetFiles(_folderPath)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f)
                    .ToArray();
                
                if (_imageFiles.Length == 0)
                {
                    ErrorOccurred?.Invoke(this, new Exception($"No image files found in: {_folderPath}"));
                    return false;
                }
                
                // Load first image to get dimensions
                using var firstImage = Cv2.ImRead(_imageFiles[0]);
                Width = firstImage.Width;
                Height = firstImage.Height;
                
                _currentIndex = 0;
                _isConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }, cancellationToken);
    }
    
    public async Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _imageFiles.Length == 0)
            return null;
            
        return await Task.Run(() =>
        {
            try
            {
                if (_currentIndex >= _imageFiles.Length)
                {
                    if (_loop)
                        _currentIndex = 0;
                    else
                        return null;
                }
                
                using var image = Cv2.ImRead(_imageFiles[_currentIndex]);
                Width = image.Width;
                Height = image.Height;
                
                var data = new byte[image.Total() * image.ElemSize()];
                System.Runtime.InteropServices.Marshal.Copy(image.Data, data, 0, data.Length);
                
                _currentIndex++;
                return data;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
                return null;
            }
        }, cancellationToken);
    }
    
    public async Task StartContinuousCaptureAsync(Action<byte[]> onFrameCaptured, CancellationToken cancellationToken = default)
    {
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
                else if (!_loop)
                {
                    break;
                }
                
                await Task.Delay((int)(1000 / Math.Max(1, FrameRate)), _continuousCts.Token);
            }
        }, _continuousCts.Token);
    }
    
    public Task StopContinuousCaptureAsync()
    {
        _continuousCts?.Cancel();
        _continuousCts?.Dispose();
        _continuousCts = null;
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Reset to the first image
    /// </summary>
    public void Reset()
    {
        _currentIndex = 0;
    }
    
    /// <summary>
    /// Go to a specific image by index
    /// </summary>
    public void GoToImage(int index)
    {
        if (index >= 0 && index < _imageFiles.Length)
            _currentIndex = index;
    }
    
    public void Dispose()
    {
        StopContinuousCaptureAsync().Wait();
        _isConnected = false;
        GC.SuppressFinalize(this);
    }
}
