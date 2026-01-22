using System.Text.Json;
using OpenCvSharp;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Vision.Tools.Base;

namespace VisionPro.Vision.Tools;

/// <summary>
/// Template/pattern matching tool using ORB/AKAZE feature detection.
/// Uses feature descriptors instead of raw image templates.
/// </summary>
public class TeachMatchTool : VisionToolBase
{
    private Mat? _teachDescriptors;
    private KeyPoint[]? _teachKeypoints;
    private Point2f[]? _teachCorners;
    private string _featureType = "ORB"; // ORB or AKAZE
    private double _matchThreshold = 0.7;
    private int _minMatches = 10;
    
    public override VisionToolType ToolType => VisionToolType.TeachMatch;
    
    /// <summary>
    /// Whether a pattern has been taught
    /// </summary>
    public bool IsTaught => _teachDescriptors != null && !_teachDescriptors.Empty();
    
    protected override void OnConfigure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("featureType", out var ftProp))
            _featureType = ftProp.GetString() ?? "ORB";
        
        if (configuration.TryGetProperty("matchThreshold", out var mtProp))
            _matchThreshold = mtProp.GetDouble();
        
        if (configuration.TryGetProperty("minMatches", out var mmProp))
            _minMatches = mmProp.GetInt32();
        
        // Load pre-saved descriptors if available
        if (configuration.TryGetProperty("descriptorsBase64", out var descProp))
        {
            var base64 = descProp.GetString();
            if (!string.IsNullOrEmpty(base64))
            {
                var bytes = Convert.FromBase64String(base64);
                _teachDescriptors = Mat.FromImageData(bytes);
            }
        }
    }
    
    /// <summary>
    /// Teach a pattern from the specified region
    /// </summary>
    public VisionToolResult Teach(byte[] imageData, int width, int height, ROI? roi = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            using var mat = CreateMat(imageData, width, height);
            using var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
            
            // Apply ROI if specified
            Mat regionToTeach = gray;
            Rect roiRect = new Rect(0, 0, width, height);
            
            if (roi != null && roi.Type == ROIType.Rectangle && roi.Points.Length >= 4)
            {
                roiRect = new Rect(
                    (int)roi.Points[0], 
                    (int)roi.Points[1], 
                    (int)roi.Points[2], 
                    (int)roi.Points[3]);
                regionToTeach = gray[roiRect];
            }
            
            // Create feature detector
            using var detector = CreateDetector();
            _teachDescriptors = new Mat();
            _teachKeypoints = detector.Detect(regionToTeach);
            detector.Compute(regionToTeach, ref _teachKeypoints, _teachDescriptors);
            
            if (_teachKeypoints == null || _teachKeypoints.Length < _minMatches)
            {
                return VisionToolResult.CreateFailure("Not enough features detected for teaching");
            }
            
            // Store corners for perspective transform
            _teachCorners = new Point2f[]
            {
                new Point2f(0, 0),
                new Point2f(regionToTeach.Width, 0),
                new Point2f(regionToTeach.Width, regionToTeach.Height),
                new Point2f(0, regionToTeach.Height)
            };
            
            stopwatch.Stop();
            
            var result = VisionToolResult.CreateSuccess();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Data["keypointCount"] = _teachKeypoints.Length;
            result.Data["teachWidth"] = regionToTeach.Width;
            result.Data["teachHeight"] = regionToTeach.Height;
            
            return result;
        }
        catch (Exception ex)
        {
            return VisionToolResult.CreateFailure($"Teaching failed: {ex.Message}");
        }
    }
    
    public override async Task<VisionToolResult> ExecuteAsync(
        byte[] imageData,
        int width,
        int height,
        ROI? roi = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            if (!IsTaught)
            {
                return VisionToolResult.CreateFailure("No pattern has been taught");
            }
            
            try
            {
                using var mat = CreateMat(imageData, width, height);
                using var gray = new Mat();
                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                
                // Detect features in current image
                using var detector = CreateDetector();
                using var descriptors = new Mat();
                var keypoints = detector.Detect(gray);
                detector.Compute(gray, ref keypoints, descriptors);
                
                if (keypoints == null || keypoints.Length < _minMatches)
                {
                    return new VisionToolResult
                    {
                        Success = false,
                        Status = InspectionStatus.NG,
                        ErrorMessage = "Not enough features in image"
                    };
                }
                
                // Match features
                using var matcher = new BFMatcher(_featureType == "ORB" ? NormTypes.Hamming : NormTypes.L2);
                var matches = matcher.KnnMatch(_teachDescriptors!, descriptors, 2);
                
                // Apply ratio test
                var goodMatches = new List<DMatch>();
                foreach (var match in matches)
                {
                    if (match.Length >= 2 && match[0].Distance < _matchThreshold * match[1].Distance)
                    {
                        goodMatches.Add(match[0]);
                    }
                }
                
                if (goodMatches.Count < _minMatches)
                {
                    return new VisionToolResult
                    {
                        Success = false,
                        Status = InspectionStatus.NG,
                        ErrorMessage = $"Not enough good matches: {goodMatches.Count} < {_minMatches}"
                    };
                }
                
                // Find homography
                var srcPoints = goodMatches.Select(m => _teachKeypoints![m.QueryIdx].Pt).ToArray();
                var dstPoints = goodMatches.Select(m => keypoints[m.TrainIdx].Pt).ToArray();
                
                using var srcPointsMat = InputArray.Create(srcPoints);
                using var dstPointsMat = InputArray.Create(dstPoints);
                using var H = Cv2.FindHomography(srcPointsMat, dstPointsMat, HomographyMethods.Ransac);
                
                if (H.Empty())
                {
                    return VisionToolResult.CreateFailure("Could not compute homography");
                }
                
                // Transform corners to get position
                var transformedCorners = Cv2.PerspectiveTransform(_teachCorners, H);
                
                // Calculate center position and rotation
                var center = new Point2f(
                    transformedCorners.Average(p => p.X),
                    transformedCorners.Average(p => p.Y));
                
                // Calculate rotation from the top edge
                var dx = transformedCorners[1].X - transformedCorners[0].X;
                var dy = transformedCorners[1].Y - transformedCorners[0].Y;
                var rotation = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                
                // Calculate scale
                var originalWidth = Math.Sqrt(
                    Math.Pow(_teachCorners![1].X - _teachCorners[0].X, 2) +
                    Math.Pow(_teachCorners[1].Y - _teachCorners[0].Y, 2));
                var currentWidth = Math.Sqrt(
                    Math.Pow(transformedCorners[1].X - transformedCorners[0].X, 2) +
                    Math.Pow(transformedCorners[1].Y - transformedCorners[0].Y, 2));
                var scale = currentWidth / originalWidth;
                
                // Calculate confidence (based on number of matches)
                var confidence = Math.Min(1.0, goodMatches.Count / (double)(_minMatches * 3));
                
                stopwatch.Stop();
                
                // Draw result overlay
                using var overlay = mat.Clone();
                var pts = transformedCorners.Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
                Cv2.Polylines(overlay, new[] { pts }, true, Scalar.Green, 2);
                Cv2.Circle(overlay, new Point((int)center.X, (int)center.Y), 5, Scalar.Red, -1);
                
                var outputBytes = new byte[overlay.Total() * overlay.ElemSize()];
                System.Runtime.InteropServices.Marshal.Copy(overlay.Data, outputBytes, 0, outputBytes.Length);
                
                return new VisionToolResult
                {
                    Success = true,
                    Status = InspectionStatus.OK,
                    Position = new ProductPosition
                    {
                        X = center.X,
                        Y = center.Y,
                        Rotation = rotation,
                        Scale = scale,
                        Confidence = confidence
                    },
                    OutputImage = outputBytes,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    Data =
                    {
                        ["matchCount"] = goodMatches.Count,
                        ["corners"] = transformedCorners
                    }
                };
            }
            catch (Exception ex)
            {
                return VisionToolResult.CreateFailure($"Match failed: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    /// <summary>
    /// Get the taught descriptors as Base64 for saving
    /// </summary>
    public string? GetDescriptorsBase64()
    {
        if (_teachDescriptors == null || _teachDescriptors.Empty())
            return null;
        
        Cv2.ImEncode(".png", _teachDescriptors, out var buffer);
        return Convert.ToBase64String(buffer);
    }
    
    private Feature2D CreateDetector()
    {
        return _featureType.ToUpperInvariant() switch
        {
            "AKAZE" => AKAZE.Create(),
            _ => ORB.Create(1000)
        };
    }
    
    private static Mat CreateMat(byte[] data, int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_8UC3);
        System.Runtime.InteropServices.Marshal.Copy(data, 0, mat.Data, data.Length);
        return mat;
    }
    
    public override void Dispose()
    {
        _teachDescriptors?.Dispose();
        _teachDescriptors = null;
        base.Dispose();
    }
}
