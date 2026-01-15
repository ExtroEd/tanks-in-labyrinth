namespace Client.Logic;

public static class BulletsRicochet
{
    private const double WallThicknessFactor = 0.04;
    private const double PushEpsilon = 0.5;
    private const double Damping = 0.98;

    public static bool TryReflectBullet(
        ref double cx, ref double cy,
        ref double vx, ref double vy,
        double radius,
        double cellSize,
        HashSet<(int, int, int, int)> passages,
        int mapW, int mapH)
    {
        var halfThickness = cellSize * WallThicknessFactor * 0.5;

        var minCellX = Math.Max(0, (int)Math.Floor((cx - radius - halfThickness) / cellSize) - 1);
        var maxCellX = Math.Min(mapW - 1, (int)Math.Floor((cx + radius + halfThickness) / cellSize) + 1);
        var minCellY = Math.Max(0, (int)Math.Floor((cy - radius - halfThickness) / cellSize) - 1);
        var maxCellY = Math.Min(mapH - 1, (int)Math.Floor((cy + radius + halfThickness) / cellSize) + 1);

        for (var x = minCellX; x <= maxCellX; x++)
        {
            for (var y = minCellY; y <= maxCellY; y++)
            {
                if (!passages.Contains((x, y, x + 1, y)))
                {
                    var x3 = (x + 1) * cellSize;
                    var y3 = y * cellSize;
                    var x4 = (x + 1) * cellSize;
                    var y4 = (y + 1) * cellSize;

                    if (CircleIntersectsThickSegment(cx, cy, radius, halfThickness, x3, y3, x4, y4, out var px, out var py, out var dist))
                    {
                        ApplyReflection(ref cx, ref cy, ref vx, ref vy, px, py, dist, radius, halfThickness);
                        return true;
                    }
                }

                if (!passages.Contains((x, y, x - 1, y)))
                {
                    var x3 = x * cellSize;
                    var y3 = y * cellSize;
                    var x4 = x * cellSize;
                    var y4 = (y + 1) * cellSize;

                    if (CircleIntersectsThickSegment(cx, cy, radius, halfThickness, x3, y3, x4, y4, out var px, out var py, out var dist))
                    {
                        ApplyReflection(ref cx, ref cy, ref vx, ref vy, px, py, dist, radius, halfThickness);
                        return true;
                    }
                }

                if (!passages.Contains((x, y, x, y + 1)))
                {
                    var x3 = x * cellSize;
                    var y3 = (y + 1) * cellSize;
                    var x4 = (x + 1) * cellSize;
                    var y4 = (y + 1) * cellSize;

                    if (CircleIntersectsThickSegment(cx, cy, radius, halfThickness, x3, y3, x4, y4, out var px, out var py, out var dist))
                    {
                        ApplyReflection(ref cx, ref cy, ref vx, ref vy, px, py, dist, radius, halfThickness);
                        return true;
                    }
                }

                if (passages.Contains((x, y, x, y - 1))) continue;
                {
                    var x3 = x * cellSize;
                    var y3 = y * cellSize;
                    var x4 = (x + 1) * cellSize;
                    var y4 = y * cellSize;

                    if (!CircleIntersectsThickSegment(cx, cy, radius,
                            halfThickness, x3, y3, x4, y4, out var px,
                            out var py, out var dist)) continue;
                    ApplyReflection(ref cx, ref cy, ref vx, ref vy, px, py, dist, radius, halfThickness);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CircleIntersectsThickSegment(
        double cx, double cy, double radius, double halfThickness,
        double x3, double y3, double x4, double y4,
        out double px, out double py, out double dist)
    {
        var vx = x4 - x3;
        var vy = y4 - y3;
        var len2 = vx * vx + vy * vy;
        if (len2 == 0)
        {
            px = x3; py = y3;
        }
        else
        {
            var t = ((cx - x3) * vx + (cy - y3) * vy) / len2;
            t = t switch
            {
                < 0 => 0,
                > 1 => 1,
                _ => t
            };
            px = x3 + t * vx;
            py = y3 + t * vy;
        }

        var dx = cx - px;
        var dy = cy - py;
        dist = Math.Sqrt(dx * dx + dy * dy);
        return dist <= radius + halfThickness;
    }

    private static void ApplyReflection(
        ref double cx, ref double cy,
        ref double vx, ref double vy,
        double px, double py, double dist, double radius, double halfThickness)
    {
        var nx = cx - px;
        var ny = cy - py;
        var nLen = Math.Sqrt(nx * nx + ny * ny);

        if (nLen < 1e-6)
        {
            nx = -vy;
            ny = vx;
            nLen = Math.Sqrt(nx * nx + ny * ny);
            if (nLen < 1e-6)
            {
                nx = 0;
                ny = -1;
                nLen = 1;
            }
        }

        nx /= nLen;
        ny /= nLen;

        var dot = vx * nx + vy * ny;
        vx -= 2 * dot * nx;
        vy -= 2 * dot * ny;

        vx *= Damping;
        vy *= Damping;

        var overlap = (radius + halfThickness) - dist;
        if (overlap < 0) overlap = 0;
        var push = overlap + PushEpsilon;

        cx += nx * push;
        cy += ny * push;
    }
}
