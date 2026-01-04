using System.Windows;

namespace Client.Logic;

public static class TankGeometry
{
    public static List<Point> GetRectCorners(double cx, double cy, double angle, double w, double h, double rectW, double rectH, double marginTop)
    {
        var scale = w / 70.0;
        var sRectW = rectW * scale;
        var sRectH = rectH * scale;
        var sMarginTop = marginTop * scale;

        var rad = angle * Math.PI / 180;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);

        var offsetY = (sMarginTop + sRectH / 2.0) - (h / 2.0);

        Point[] localCorners = [
            new(-sRectW / 2, -sRectH / 2),
            new(sRectW / 2, -sRectH / 2),
            new(sRectW / 2, sRectH / 2),
            new(-sRectW / 2, sRectH / 2)
        ];

        return localCorners.Select(p => new Point(
            cx + (p.X * cos - (p.Y + offsetY) * sin),
            cy + (p.X * sin + (p.Y + offsetY) * cos)
        )).ToList();
    }

    public static bool ArePolygonsIntersecting(List<Point> a, List<Point> b)
    {
        foreach (var polygon in new[] { a, b })
        {
            for (var i = 0; i < polygon.Count; i++)
            {
                var j = (i + 1) % polygon.Count;
                Point side = new(polygon[j].Y - polygon[i].Y, polygon[i].X - polygon[j].X);

                double minA = double.MaxValue, maxA = double.MinValue;
                foreach (var dot in a.Select(p => p.X * side.X + p.Y * side.Y))
                {
                    minA = Math.Min(minA, dot); maxA = Math.Max(maxA, dot);
                }

                double minB = double.MaxValue, maxB = double.MinValue;
                foreach (var dot in b.Select(p => p.X * side.X + p.Y * side.Y))
                {
                    minB = Math.Min(minB, dot); maxB = Math.Max(maxB, dot);
                }

                if (maxA < minB || maxB < minA) return false;
            }
        }
        return true;
    }
}
