using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.Logic;

public static class TankTankCollision
{
    public static TankState? GetCollidingTank(UIElement self, double cx, double cy, double angle, double w, double h)
    {
        var selfCorners = TankGeometry.GetRectCorners(cx, cy, angle, w, h, 34, 46, 12);

        return (from other in TankRegistry.Tanks
                where other.Visual != self && other.IsAlive
                let otherCorners = TankGeometry.GetRectCorners(other.X, other.Y, other.Angle, other.Width, other.Height, 34, 46, 12)
                where TankGeometry.ArePolygonsIntersecting(selfCorners, otherCorners)
                select other).FirstOrDefault();
    }
    
    public static bool IsHittingAnyTank(UIElement self, double cx, double cy, double angle, double w, double h)
    {
        return GetCollidingTank(self, cx, cy, angle, w, h) != null;
    }
    
    public static bool TryPush(
        TankState other, 
        double vx, double vy, 
        double cellSize, 
        HashSet<(int, int, int, int)> passages, 
        int mapW, int mapH,
        double selfX, double selfY)
    {
        var nextOtherX = other.X + vx * 0.8;
        var nextOtherY = other.Y + vy * 0.8;

        var dx = other.X - selfX;
        var dy = other.Y - selfY;
        
        var moveDir = Math.Atan2(vy, vx);
        var toTargetDir = Math.Atan2(dy, dx);
        
        var torque = Math.Sin(moveDir - toTargetDir);
        
        if (Math.Abs(torque) < 0.1) 
        {
            torque = (Random.Shared.NextDouble() - 0.5) * 0.2;
        }

        var nextAngle = (other.Angle + torque * 8.0) % 360;

        if (TankWallCollision.IsCollidingWithWall(nextOtherX, nextOtherY, nextAngle, other.Width, other.Height, cellSize, passages, mapW, mapH))
        {
            nextOtherX = other.X;
            nextOtherY = other.Y;
            
            if (TankWallCollision.IsCollidingWithWall(nextOtherX, nextOtherY, nextAngle, other.Width, other.Height, cellSize, passages, mapW, mapH))
                return false;
        }

        if (GetCollidingTank(other.Visual, nextOtherX, nextOtherY, nextAngle, other.Width, other.Height) != null)
            return false;

        Canvas.SetLeft(other.Visual, nextOtherX - other.Width / 2);
        Canvas.SetTop(other.Visual, nextOtherY - other.Height / 2);

        other.Visual.RenderTransform = new RotateTransform(nextAngle, other.Width / 2, other.Height / 2);

        TankRegistry.UpdateState(other.Visual, nextOtherX, nextOtherY, nextAngle);
        
        return true;
    }
}
