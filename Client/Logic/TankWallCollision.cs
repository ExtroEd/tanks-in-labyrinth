using System.Windows;

namespace Client.Logic;

public static class TankWallCollision
{
    private const double WallThicknessFactor = 0.04;

    public static bool IsCollidingWithWall(double cx, double cy, double angle, double w, double h, double size, HashSet<(int, int, int, int)> passages, int mapW, int mapH)
    {
        var body = TankGeometry.GetRectCorners(cx, cy, angle, w, h, 34, 46, 12);
        var gun = TankGeometry.GetRectCorners(cx, cy, angle, w, h, 8, 25, 3);

        if (IsOutside(body, size, mapW, mapH) || IsOutside(gun, size, mapW, mapH)) return true;

        return CheckWallLines(body, size, passages) || CheckWallLines(gun, size, passages);
    }

    private static bool IsOutside(List<Point> corners, double size, int mapW, int mapH)
    {
        return corners.Any(p => p.X < 2 || p.Y < 2 || p.X > mapW * size - 2 || p.Y > mapH * size - 2);
    }

    private static bool CheckWallLines(List<Point> corners, double size, HashSet<(int, int, int, int)> passages)
    {
        var halfThickness = size * WallThicknessFactor * 0.5;

        for (var i = 0; i < 4; i++)
        {
            Point p1 = corners[i], p2 = corners[(i + 1) % 4];
            int minX = (int)(Math.Min(p1.X, p2.X) / size), maxX = (int)(Math.Max(p1.X, p2.X) / size);
            int minY = (int)(Math.Min(p1.Y, p2.Y) / size), maxY = (int)(Math.Max(p1.Y, p2.Y) / size);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    if (!passages.Contains((x, y, x + 1, y)))
                    {
                        var wx1 = (x + 1) * size;
                        var wy1 = y * size;
                        var wx2 = (x + 1) * size;
                        var wy2 = (y + 1) * size;

                        if (SegmentDistance(p1, p2, new Point(wx1, wy1), new Point(wx2, wy2)) <= halfThickness) return true;
                    }

                    if (!passages.Contains((x, y, x - 1, y)))
                    {
                        var wx1 = x * size;
                        var wy1 = y * size;
                        var wx2 = x * size;
                        var wy2 = (y + 1) * size;

                        if (SegmentDistance(p1, p2, new Point(wx1, wy1), new Point(wx2, wy2)) <= halfThickness) return true;
                    }

                    if (!passages.Contains((x, y, x, y + 1)))
                    {
                        var wx1 = x * size;
                        var wy1 = (y + 1) * size;
                        var wx2 = (x + 1) * size;
                        var wy2 = (y + 1) * size;

                        if (SegmentDistance(p1, p2, new Point(wx1, wy1), new Point(wx2, wy2)) <= halfThickness) return true;
                    }

                    if (passages.Contains((x, y, x, y - 1))) continue;
                    {
                        var wx1 = x * size;
                        var wy1 = y * size;
                        var wx2 = (x + 1) * size;
                        var wy2 = y * size;

                        if (SegmentDistance(p1, p2, new Point(wx1, wy1), new Point(wx2, wy2)) <= halfThickness) return true;
                    }
                }
            }
        }
        return false;
    }

    private static double SegmentDistance(Point a1, Point a2, Point b1, Point b2)
    {
        if (Intersects(a1, a2, b1.X, b1.Y, b2.X, b2.Y)) return 0.0;

        var d1 = PointSegmentDistance(a1, b1, b2);
        var d2 = PointSegmentDistance(a2, b1, b2);
        var d3 = PointSegmentDistance(b1, a1, a2);
        var d4 = PointSegmentDistance(b2, a1, a2);

        return Math.Min(Math.Min(d1, d2), Math.Min(d3, d4));
    }

    private static double PointSegmentDistance(Point p, Point a, Point b)
    {
        var vx = b.X - a.X;
        var vy = b.Y - a.Y;
        var len2 = vx * vx + vy * vy;
        if (len2 == 0)
        {
            var dx0 = p.X - a.X;
            var dy0 = p.Y - a.Y;
            return Math.Sqrt(dx0 * dx0 + dy0 * dy0);
        }

        var t = ((p.X - a.X) * vx + (p.Y - a.Y) * vy) / len2;
        t = t switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => t
        };
        var projX = a.X + t * vx;
        var projY = a.Y + t * vy;
        var dx = p.X - projX;
        var dy = p.Y - projY;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static bool Intersects(Point a, Point b, double x3, double y3, double x4, double y4)
    {
        var den = (y4 - y3) * (b.X - a.X) - (x4 - x3) * (b.Y - a.Y);
        if (den == 0) return false;
        var ua = ((x4 - x3) * (a.Y - y3) - (y4 - y3) * (a.X - x3)) / den;
        var ub = ((b.X - a.X) * (a.Y - y3) - (b.Y - a.Y) * (a.X - x3)) / den;
        return ua is >= 0 and <= 1 && ub is >= 0 and <= 1;
    }
}
