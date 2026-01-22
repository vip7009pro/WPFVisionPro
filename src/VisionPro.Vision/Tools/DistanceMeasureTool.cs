using System.Text.Json;
using OpenCvSharp;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Vision.Tools.Base;

namespace VisionPro.Vision.Tools;

/// <summary>
/// Distance measurement tool for point-to-point and line-to-line measurements
/// </summary>
public class DistanceMeasureTool : VisionToolBase
{
    private MeasurementSpec? _spec;
    private Point2d _point1;
    private Point2d _point2;
    private bool _useEdgeDetection = true;
    private int _edgeThreshold = 50;
    
    public override VisionToolType ToolType => VisionToolType.DistanceMeasure;
    
    protected override void OnConfigure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("point1", out var p1))
        {
            _point1 = new Point2d(
                p1.GetProperty("x").GetDouble(),
                p1.GetProperty("y").GetDouble());
        }
        
        if (configuration.TryGetProperty("point2", out var p2))
        {
            _point2 = new Point2d(
                p2.GetProperty("x").GetDouble(),
                p2.GetProperty("y").GetDouble());
        }
        
        if (configuration.TryGetProperty("useEdgeDetection", out var useProp))
            _useEdgeDetection = useProp.GetBoolean();
        
        if (configuration.TryGetProperty("edgeThreshold", out var threshProp))
            _edgeThreshold = threshProp.GetInt32();
        
        // Load measurement spec
        if (configuration.TryGetProperty("spec", out var specProp))
        {
            _spec = new MeasurementSpec
            {
                Id = specProp.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                Name = specProp.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                Nominal = specProp.TryGetProperty("nominal", out var nom) ? nom.GetDouble() : 0,
                TolerancePlus = specProp.TryGetProperty("tolerancePlus", out var tolP) ? tolP.GetDouble() : 0,
                ToleranceMinus = specProp.TryGetProperty("toleranceMinus", out var tolM) ? tolM.GetDouble() : 0,
                Unit = specProp.TryGetProperty("unit", out var unit) ? unit.GetString() ?? "mm" : "mm",
                PixelsPerUnit = specProp.TryGetProperty("pixelsPerUnit", out var ppu) ? ppu.GetDouble() : 1
            };
        }
    }
    
    /// <summary>
    /// Set measurement points
    /// </summary>
    public void SetPoints(Point2d point1, Point2d point2)
    {
        _point1 = point1;
        _point2 = point2;
    }
    
    /// <summary>
    /// Set measurement specification
    /// </summary>
    public void SetSpec(MeasurementSpec spec)
    {
        _spec = spec;
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
            
            try
            {
                using var mat = CreateMat(imageData, width, height);
                
                Point2d actualPoint1 = _point1;
                Point2d actualPoint2 = _point2;
                
                // If edge detection is enabled, refine the points
                if (_useEdgeDetection)
                {
                    using var gray = new Mat();
                    Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                    
                    using var edges = new Mat();
                    Cv2.Canny(gray, edges, _edgeThreshold, _edgeThreshold * 2);
                    
                    actualPoint1 = FindNearestEdge(edges, _point1);
                    actualPoint2 = FindNearestEdge(edges, _point2);
                }
                
                // Calculate distance in pixels
                var distancePixels = Math.Sqrt(
                    Math.Pow(actualPoint2.X - actualPoint1.X, 2) +
                    Math.Pow(actualPoint2.Y - actualPoint1.Y, 2));
                
                // Convert to real units
                var pixelsPerUnit = _spec?.PixelsPerUnit ?? 1.0;
                var distanceUnits = distancePixels / pixelsPerUnit;
                
                // Create measurement result
                var measurement = new Measurement
                {
                    Id = _spec?.Id ?? Guid.NewGuid().ToString(),
                    Name = _spec?.Name ?? "Distance",
                    Value = distanceUnits,
                    Unit = _spec?.Unit ?? "px",
                    Nominal = _spec?.Nominal ?? 0,
                    TolerancePlus = _spec?.TolerancePlus ?? double.MaxValue,
                    ToleranceMinus = _spec?.ToleranceMinus ?? double.MaxValue
                };
                
                measurement.Status = measurement.IsWithinTolerance() 
                    ? InspectionStatus.OK 
                    : InspectionStatus.NG;
                
                stopwatch.Stop();
                
                // Draw measurement on overlay
                using var overlay = mat.Clone();
                Cv2.Line(overlay, 
                    new Point((int)actualPoint1.X, (int)actualPoint1.Y),
                    new Point((int)actualPoint2.X, (int)actualPoint2.Y),
                    measurement.Status == InspectionStatus.OK ? Scalar.Green : Scalar.Red, 2);
                
                Cv2.Circle(overlay, new Point((int)actualPoint1.X, (int)actualPoint1.Y), 5, Scalar.Blue, -1);
                Cv2.Circle(overlay, new Point((int)actualPoint2.X, (int)actualPoint2.Y), 5, Scalar.Blue, -1);
                
                // Add text label
                var midPoint = new Point(
                    (int)((actualPoint1.X + actualPoint2.X) / 2),
                    (int)((actualPoint1.Y + actualPoint2.Y) / 2) - 10);
                Cv2.PutText(overlay, $"{distanceUnits:F2} {measurement.Unit}", midPoint,
                    HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 2);
                
                var outputBytes = new byte[overlay.Total() * overlay.ElemSize()];
                System.Runtime.InteropServices.Marshal.Copy(overlay.Data, outputBytes, 0, outputBytes.Length);
                
                return new VisionToolResult
                {
                    Success = true,
                    Status = measurement.Status,
                    Measurements = new List<Measurement> { measurement },
                    OutputImage = outputBytes,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    Data =
                    {
                        ["distancePixels"] = distancePixels,
                        ["point1"] = actualPoint1,
                        ["point2"] = actualPoint2
                    }
                };
            }
            catch (Exception ex)
            {
                return VisionToolResult.CreateFailure($"Measurement failed: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    private Point2d FindNearestEdge(Mat edges, Point2d point, int searchRadius = 20)
    {
        var searchRect = new Rect(
            Math.Max(0, (int)point.X - searchRadius),
            Math.Max(0, (int)point.Y - searchRadius),
            Math.Min(searchRadius * 2, edges.Width - (int)point.X + searchRadius),
            Math.Min(searchRadius * 2, edges.Height - (int)point.Y + searchRadius));
        
        if (searchRect.Width <= 0 || searchRect.Height <= 0)
            return point;
        
        // Find edge points in search region
        using var region = edges[searchRect];
        using var points = new Mat();
        Cv2.FindNonZero(region, points);
        
        if (points.Empty() || points.Rows == 0)
            return point;
        
        // Find closest point to original
        var minDist = double.MaxValue;
        var nearestPoint = point;
        
        for (int i = 0; i < points.Rows; i++)
        {
            var edgePoint = points.At<Point>(i);
            var globalPoint = new Point2d(
                searchRect.X + edgePoint.X,
                searchRect.Y + edgePoint.Y);
            
            var dist = Math.Sqrt(
                Math.Pow(globalPoint.X - point.X, 2) +
                Math.Pow(globalPoint.Y - point.Y, 2));
            
            if (dist < minDist)
            {
                minDist = dist;
                nearestPoint = globalPoint;
            }
        }
        
        return nearestPoint;
    }
    
    private static Mat CreateMat(byte[] data, int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_8UC3);
        System.Runtime.InteropServices.Marshal.Copy(data, 0, mat.Data, data.Length);
        return mat;
    }
}
