using System.Windows;

namespace Client.Logic;

public static class TankWallCollision
{
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
        for (var i = 0; i < 4; i++)
        {
            Point p1 = corners[i], p2 = corners[(i + 1) % 4];
            int minX = (int)(Math.Min(p1.X, p2.X) / size), maxX = (int)(Math.Max(p1.X, p2.X) / size);
            int minY = (int)(Math.Min(p1.Y, p2.Y) / size), maxY = (int)(Math.Max(p1.Y, p2.Y) / size);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    if (!passages.Contains((x, y, x + 1, y)) && Intersects(p1, p2, (x + 1) * size, y * size, (x + 1) * size, (y + 1) * size)) return true;
                    if (!passages.Contains((x, y, x - 1, y)) && Intersects(p1, p2, x * size, y * size, x * size, (y + 1) * size)) return true;
                    if (!passages.Contains((x, y, x, y + 1)) && Intersects(p1, p2, x * size, (y + 1) * size, (x + 1) * size, (y + 1) * size)) return true;
                    if (!passages.Contains((x, y, x, y - 1)) && Intersects(p1, p2, x * size, y * size, (x + 1) * size, y * size)) return true;
                }
            }
        }
        return false;
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
