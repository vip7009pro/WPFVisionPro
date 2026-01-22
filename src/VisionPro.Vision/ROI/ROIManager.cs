using OpenCvSharp;
using VisionPro.Core.Enums;
using VisionPro.Core.Models;

namespace VisionPro.Vision.ROIUtils;

/// <summary>
/// Manages ROI operations and mask generation
/// </summary>
public static class ROIManager
{
    /// <summary>
    /// Creates a binary mask from a collection of ROIs
    /// </summary>
    /// <param name="rois">List of ROIs to process</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <returns>Binary mask (255 = include, 0 = exclude)</returns>
    public static Mat CreateMask(IEnumerable<Core.Models.ROI> rois, int width, int height)
    {
        var mask = new Mat(height, width, MatType.CV_8UC1, Scalar.All(255));
        
        if (rois == null || !rois.Any())
            return mask;
            
        // First, apply all INCLUDES. If there are includes, start with black mask.
        var includes = rois.Where(r => r.Usage == ROIUsage.Include && r.Enabled).ToList();
        var excludes = rois.Where(r => r.Usage == ROIUsage.Exclude && r.Enabled).ToList();
        
        if (includes.Any())
        {
            mask.SetTo(Scalar.All(0));
            foreach (var roi in includes)
            {
                DrawROI(mask, roi, Scalar.All(255));
            }
        }
        
        // Then subtract EXCLUDES
        foreach (var roi in excludes)
        {
            DrawROI(mask, roi, Scalar.All(0));
        }
        
        return mask;
    }
    
    /// <summary>
    /// Extracts the sub-image defined by the ROI.
    /// Primarily for Rectangle ROIs. For others, it returns the bounding rect crop with mask applied.
    /// </summary>
    public static Mat ExtractROI(Mat source, Core.Models.ROI roi)
    {
        if (roi.Type == ROIType.Rectangle) // Rotated Rectangle
        {
            var center = new Point2f((float)roi.Points[0], (float)roi.Points[1]);
            var size = new Size2f((float)roi.Points[2], (float)roi.Points[3]);
            var angle = (float)roi.Rotation;
            
            var rotatedRect = new RotatedRect(center, size, angle);
            
            // Get bounding rect to crop first (optimization)
            var boundingRect = rotatedRect.BoundingRect();
            // intersect with image bounds
            boundingRect = boundingRect.Intersect(new Rect(0, 0, source.Width, source.Height));
            
            if (boundingRect.Width <= 0 || boundingRect.Height <= 0)
                return new Mat();
                
            // If angle is 0, just crop
            if (Math.Abs(angle) < 0.1)
                return source[boundingRect];
                
            // Warp affine for rotation
            // TODO: precise rotation crop
            return source[boundingRect]; 
        }
        
        // For other shapes, just cut the bounding box? 
        // Or apply mask? 
        // Usually ExtractROI is for Template Matching or OCR where we want a rectangular straight image.
        
        return source.Clone();
    }
    
    private static void DrawROI(Mat mask, Core.Models.ROI roi, Scalar color)
    {
        switch (roi.Type)
        {
            case ROIType.Rectangle:
                DrawRectangle(mask, roi, color);
                break;
            case ROIType.Circle:
                DrawCircle(mask, roi, color);
                break;
            case ROIType.Polygon:
                DrawPolygon(mask, roi, color);
                break;
        }
    }
    
    private static void DrawRectangle(Mat mask, Core.Models.ROI roi, Scalar color)
    {
        if (roi.Points.Length < 4) return;
        
        var center = new Point2f((float)roi.Points[0], (float)roi.Points[1]);
        var size = new Size2f((float)roi.Points[2], (float)roi.Points[3]);
        var angle = (float)roi.Rotation;
        
        var rotatedRect = new RotatedRect(center, size, angle);
        var vertices = rotatedRect.Points().Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
        
        Cv2.FillConvexPoly(mask, vertices, color);
    }
    
    private static void DrawCircle(Mat mask, Core.Models.ROI roi, Scalar color)
    {
        if (roi.Points.Length < 3) return;
        
        var center = new Point((int)roi.Points[0], (int)roi.Points[1]);
        var radius = (int)roi.Points[2];
        
        Cv2.Circle(mask, center, radius, color, -1);
    }
    
    private static void DrawPolygon(Mat mask, Core.Models.ROI roi, Scalar color)
    {
        if (roi.Points.Length < 4 || roi.Points.Length % 2 != 0) return;
        
        var points = new List<Point>();
        for (int i = 0; i < roi.Points.Length; i += 2)
        {
            points.Add(new Point((int)roi.Points[i], (int)roi.Points[i + 1]));
        }
        
        Cv2.FillPoly(mask, new[] { points }, color);
    }
}
