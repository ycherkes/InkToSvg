using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;

namespace InkToSvg
{
    class CanvasGeometryToSvgPathReader : ICanvasPathReceiver
    {
        private readonly Vector2 _ratio;

        private List<string> Parts { get; }

        public string Path => string.Join(" ", Parts);

        public CanvasGeometryToSvgPathReader() : this(Vector2.One)
        {}

        public CanvasGeometryToSvgPathReader(Vector2 ratio)
        {
            _ratio = ratio;
            Parts = new List<string>();
        }

        public void BeginFigure(Vector2 point, CanvasFigureFill fill)
        {
            Parts.Add($"M{point.X / _ratio.X} {point.Y / _ratio.Y}");
        }
        public void AddArc(Vector2 point, float x, float y, float z, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
        {
            // Ignored
        }
        public void AddCubicBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            Parts.Add($"C{controlPoint1.X / _ratio.X},{controlPoint1.Y / _ratio.Y} {controlPoint2.X / _ratio.X},{controlPoint2.Y / _ratio.Y} {endPoint.X / _ratio.X},{endPoint.Y / _ratio.Y}");
        }

        public void AddLine(Vector2 endPoint)
        {
            Parts.Add($"L {endPoint.X / _ratio.X} {endPoint.Y / _ratio.Y}");
        }

        public void AddQuadraticBezier(Vector2 controlPoint, Vector2 endPoint)
        {
            // Ignored
        }

        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination)
        {
            // Ignored
        }

        public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions)
        {
            // Ignored
        }

        public void EndFigure(CanvasFigureLoop figureLoop)
        {
            Parts.Add("Z");
        }
    }
}
