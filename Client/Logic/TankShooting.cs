using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Client.Logic;

public class TankShooting
{
    private readonly Canvas _canvas;
    private readonly double _cellSize;
    private readonly int _mapW;
    private readonly int _mapH;
    private readonly HashSet<(int, int, int, int)> _passages;
    private readonly List<Bullet> _bullets;
    private DateTime _lastUpdate = DateTime.Now;

    private const double BulletSpeedCellsPerSec = 2;
    private const double BulletLifetimeSeconds = 15.0;
    private const double FireCooldownSecondsPerTank = 0.1;
    private const double OwnerCollisionIgnoreSeconds = 0.1;
    private const int MaxRicochetsPerBullet = 20;
    private const int MaxActiveBullets = 5;
    private readonly Dictionary<UIElement, DateTime> _lastShotAt = new();
    private readonly RoundManager _roundManager;

    public event Action<TankState, UIElement?>? TankHit;

    public TankShooting(Canvas canvas, double cellSize, int mapW, int mapH, HashSet<(int, int, int, int)> passages, RoundManager roundManager)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _cellSize = cellSize;
        _mapW = mapW;
        _mapH = mapH;
        _passages = passages ?? throw new ArgumentNullException(nameof(passages));
        _roundManager = roundManager; // Сохраняем ссылку
        _bullets = [];

        CompositionTarget.Rendering += OnUpdate;
    }

    public void Shoot(UIElement? tankVisual)
    {
        if (tankVisual == null) return;
        if (_bullets.Count >= MaxActiveBullets) return;
        if (_lastShotAt.TryGetValue(tankVisual, out var t) && (DateTime.Now - t).TotalSeconds < FireCooldownSecondsPerTank) return;

        var state = TankRegistry.Tanks.FirstOrDefault(x => x.Visual == tankVisual && x.IsAlive);
        if (state == null) return;

        var angleDeg = state.Angle;
        var rad = (angleDeg - 90.0) * Math.PI / 180.0;

        var speed = BulletSpeedCellsPerSec * _cellSize;
        var thickness = Math.Max(1.0, 0.1 * _cellSize);
        var radius = thickness / 2.0;

        var gunCorners = TankGeometry.GetRectCorners(state.X, state.Y, state.Angle, state.Width, state.Height, 8, 25, 3);

        var vxUnit = Math.Cos(rad);
        var vyUnit = Math.Sin(rad);

        var dots = gunCorners.Select(p => p.X * vxUnit + p.Y * vyUnit).ToList();
        var maxDot = dots.Max();
        var frontPoints = gunCorners.Where((_, i) => Math.Abs(dots[i] - maxDot) < 1e-6).ToList();
        if (frontPoints.Count == 0)
        {
            var idx = dots.IndexOf(maxDot);
            frontPoints.Add(gunCorners[idx]);
        }

        var tipX = frontPoints.Average(p => p.X);
        var tipY = frontPoints.Average(p => p.Y);

        const double startInsetTowardCenter = 0.0;
        var startX = tipX - vxUnit * startInsetTowardCenter;
        var startY = tipY - vyUnit * startInsetTowardCenter;

        var ell = new Ellipse
        {
            Width = thickness,
            Height = thickness,
            Fill = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
            IsHitTestVisible = false,
            SnapsToDevicePixels = true
        };

        Canvas.SetLeft(ell, startX - thickness / 2.0);
        Canvas.SetTop(ell, startY - thickness / 2.0);
        Panel.SetZIndex(ell, 1000);

        _canvas.Children.Add(ell);

        var b = new Bullet
        {
            Visual = ell,
            X = startX,
            Y = startY,
            Vx = vxUnit * speed,
            Vy = vyUnit * speed,
            Radius = radius,
            SpawnedAt = DateTime.Now,
            Owner = tankVisual,
            RemainingRicochets = MaxRicochetsPerBullet
        };

        _bullets.Add(b);
        _lastShotAt[tankVisual] = DateTime.Now;
    }

    private void OnUpdate(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var dt = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        if (dt <= 0) return;

        var remove = new List<Bullet>();

        foreach (var b in _bullets.ToList())
        {
            b.X += b.Vx * dt;
            b.Y += b.Vy * dt;

            var reflected = BulletsRicochet.TryReflectBullet(
                ref b.X, ref b.Y, ref b.Vx, ref b.Vy,
                b.Radius, _cellSize, _passages, _mapW, _mapH);

            if (reflected)
            {
                b.RemainingRicochets--;
                if (b.RemainingRicochets < 0)
                {
                    remove.Add(b);
                    continue;
                }
            }

            Canvas.SetLeft(b.Visual, b.X - b.Radius);
            Canvas.SetTop(b.Visual, b.Y - b.Radius);

            if ((now - b.SpawnedAt).TotalSeconds > BulletLifetimeSeconds ||
                b.X < -20 || b.Y < -20 || b.X > _mapW * _cellSize + 20 || b.Y > _mapH * _cellSize + 20)
            {
                remove.Add(b);
                continue;
            }

            var bulletPoly = MakeCircleApproximation(b.X, b.Y, b.Radius, 8);

            TankState? hitTarget = null;
            foreach (var t in TankRegistry.Tanks.Where(t => t.IsAlive))
            {
                if (t.Visual == b.Owner && (now - b.SpawnedAt).TotalSeconds < OwnerCollisionIgnoreSeconds)
                    continue;

                var body = TankGeometry.GetRectCorners(t.X, t.Y, t.Angle, t.Width, t.Height, 34, 46, 12);
                if (TankGeometry.ArePolygonsIntersecting(body, bulletPoly))
                {
                    hitTarget = t;
                    break;
                }

                var gun = TankGeometry.GetRectCorners(t.X, t.Y, t.Angle, t.Width, t.Height, 8, 25, 3);
                if (!TankGeometry.ArePolygonsIntersecting(gun, bulletPoly))
                    continue;
                hitTarget = t;
                break;
            }

            if (hitTarget == null) continue;
            try
            {
                if (b.Owner != null && b.Owner != hitTarget.Visual)
                {
                    var ownerState = TankRegistry.Tanks.FirstOrDefault(x => x.Visual == b.Owner);
                    if (ownerState != null)
                    {
                        ownerState.Kills++;
                    }
                }
                hitTarget.IsAlive = false; 
                _canvas.Children.Remove(hitTarget.Visual);
                
                TankHit?.Invoke(hitTarget, b.Owner);
                
                _roundManager.CheckRoundCondition();
            }
            catch
            {
                // ignored
            }

            remove.Add(b);
        }

        foreach (var b in remove)
        {
            _canvas.Children.Remove(b.Visual);
            _bullets.Remove(b);
        }
    }

    private static List<Point> MakeCircleApproximation(double cx, double cy, double r, int steps)
    {
        var pts = new List<Point>(steps);
        for (var i = 0; i < steps; i++)
        {
            var a = 2.0 * Math.PI * i / steps;
            pts.Add(new Point(cx + Math.Cos(a) * r, cy + Math.Sin(a) * r));
        }
        return pts;
    }

    private class Bullet
    {
        public Ellipse Visual = null!;
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Radius;
        public DateTime SpawnedAt;
        public UIElement? Owner;
        public int RemainingRicochets;
    }
    
    public void ClearBullets()
    {
        foreach (var b in _bullets)
        {
            _canvas.Children.Remove(b.Visual);
        }

        _bullets.Clear();
    }
    
    public void Cleanup()
    {
        // ОЧЕНЬ ВАЖНО: Останавливаем старый цикл обновления
        CompositionTarget.Rendering -= OnUpdate;
    
        // Удаляем визуал пуль
        ClearBullets();
    }
}
