using System.Text.Json;
using OpenCvSharp;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;
using VisionPro.Vision.Tools.Base;

namespace VisionPro.Vision.Tools;

/// <summary>
/// Defect detection tool for finding white/black spots using binary threshold and morphology
/// </summary>
public class DefectDetectionTool : VisionToolBase
{
    private int _binaryThreshold = 128;
    private double _minArea = 10;
    private double _maxArea = 10000;
    private int _morphologyKernel = 3;
    private bool _detectWhite = true;
    private bool _detectBlack = true;
    private double _circularityMin = 0;
    private double _circularityMax = 1;
    
    public override VisionToolType ToolType => VisionToolType.DefectDetection;
    
    protected override void OnConfigure(JsonElement configuration)
    {
        if (configuration.TryGetProperty("binaryThreshold", out var btProp))
            _binaryThreshold = btProp.GetInt32();
        
        if (configuration.TryGetProperty("minArea", out var minProp))
            _minArea = minProp.GetDouble();
        
        if (configuration.TryGetProperty("maxArea", out var maxProp))
            _maxArea = maxProp.GetDouble();
        
        if (configuration.TryGetProperty("morphologyKernel", out var mkProp))
            _morphologyKernel = mkProp.GetInt32();
        
        if (configuration.TryGetProperty("detectWhite", out var dwProp))
            _detectWhite = dwProp.GetBoolean();
        
        if (configuration.TryGetProperty("detectBlack", out var dbProp))
            _detectBlack = dbProp.GetBoolean();
        
        if (configuration.TryGetProperty("circularityMin", out var circMinProp))
            _circularityMin = circMinProp.GetDouble();
        
        if (configuration.TryGetProperty("circularityMax", out var circMaxProp))
            _circularityMax = circMaxProp.GetDouble();
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
            var defects = new List<Defect>();
            
            try
            {
                using var mat = CreateMat(imageData, width, height);
                using var gray = new Mat();
                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                
                // Apply ROI mask if specified
                Mat workingImage = gray;
                Rect roiBounds = new Rect(0, 0, width, height);
                
                if (roi != null && roi.Type == ROIType.Rectangle && roi.Points.Length >= 4)
                {
                    roiBounds = new Rect(
                        (int)roi.Points[0],
                        (int)roi.Points[1],
                        (int)roi.Points[2],
                        (int)roi.Points[3]);
                    workingImage = gray[roiBounds];
                }
                
                // Detect white spots
                if (_detectWhite)
                {
                    var whiteDefects = DetectSpots(workingImage, true, roiBounds);
                    defects.AddRange(whiteDefects);
                }
                
                // Detect black spots
                if (_detectBlack)
                {
                    var blackDefects = DetectSpots(workingImage, false, roiBounds);
                    defects.AddRange(blackDefects);
                }
                
                stopwatch.Stop();
                
                // Draw defects on overlay
                using var overlay = mat.Clone();
                foreach (var defect in defects)
                {
                    var color = defect.Type == "WhiteSpot" ? Scalar.Yellow : Scalar.Red;
                    Cv2.Rectangle(overlay, 
                        new Rect((int)defect.X, (int)defect.Y, (int)defect.Width, (int)defect.Height),
                        color, 2);
                    
                    Cv2.PutText(overlay, 
                        $"{defect.Area:F0}px", 
                        new Point((int)defect.X, (int)defect.Y - 5),
                        HersheyFonts.HersheySimplex, 0.4, color, 1);
                }
                
                var outputBytes = new byte[overlay.Total() * overlay.ElemSize()];
                System.Runtime.InteropServices.Marshal.Copy(overlay.Data, outputBytes, 0, outputBytes.Length);
                
                var status = defects.Count > 0 ? InspectionStatus.NG : InspectionStatus.OK;
                
                return new VisionToolResult
                {
                    Success = true,
                    Status = status,
                    Defects = defects,
                    OutputImage = outputBytes,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    Data =
                    {
                        ["defectCount"] = defects.Count,
                        ["whiteSpotCount"] = defects.Count(d => d.Type == "WhiteSpot"),
                        ["blackSpotCount"] = defects.Count(d => d.Type == "BlackSpot")
                    }
                };
            }
            catch (Exception ex)
            {
                return VisionToolResult.CreateFailure($"Defect detection failed: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    private List<Defect> DetectSpots(Mat gray, bool detectWhite, Rect offset)
    {
        var defects = new List<Defect>();
        
        using var binary = new Mat();
        if (detectWhite)
        {
            Cv2.Threshold(gray, binary, _binaryThreshold, 255, ThresholdTypes.Binary);
        }
        else
        {
            Cv2.Threshold(gray, binary, _binaryThreshold, 255, ThresholdTypes.BinaryInv);
        }
        
        // Apply morphology to clean up
        if (_morphologyKernel > 0)
        {
            using var kernel = Cv2.GetStructuringElement(
                MorphShapes.Ellipse, 
                new Size(_morphologyKernel, _morphologyKernel));
            Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel);
            Cv2.MorphologyEx(binary, binary, MorphTypes.Close, kernel);
        }
        
        // Find contours
        Cv2.FindContours(binary, out var contours, out _, 
            RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        
        foreach (var contour in contours)
        {
            var area = Cv2.ContourArea(contour);
            
            // Filter by area
            if (area < _minArea || area > _maxArea)
                continue;
            
            // Calculate circularity
            var perimeter = Cv2.ArcLength(contour, true);
            var circularity = perimeter > 0 ? 4 * Math.PI * area / (perimeter * perimeter) : 0;
            
            // Filter by circularity
            if (circularity < _circularityMin || circularity > _circularityMax)
                continue;
            
            var boundingRect = Cv2.BoundingRect(contour);
            
            defects.Add(new Defect
            {
                Type = detectWhite ? "WhiteSpot" : "BlackSpot",
                X = boundingRect.X + offset.X,
                Y = boundingRect.Y + offset.Y,
                Width = boundingRect.Width,
                Height = boundingRect.Height,
                Area = area,
                Severity = area > (_maxArea / 2) ? "High" : area > (_maxArea / 4) ? "Medium" : "Low"
            });
        }
        
        return defects;
    }
    
    private static Mat CreateMat(byte[] data, int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_8UC3);
        System.Runtime.InteropServices.Marshal.Copy(data, 0, mat.Data, data.Length);
        return mat;
    }
}
