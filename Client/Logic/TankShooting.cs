using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Client.Logic;

public class TankShooting
{
    private readonly Canvas _canvas;
    private readonly double _cellSize;
    private readonly int _mapW;
    private readonly int _mapH;
    private readonly List<Bullet> _bullets = new();
    private DateTime _lastUpdate = DateTime.Now;

    // Настройки (можно подправить)
    private const double BulletSpeedCellsPerSec = 8.0; // скорость в клетках/сек
    private const double BulletLifetimeSeconds = 4.0;
    private const double FireCooldownSecondsPerTank = 0.25;
    private readonly Dictionary<object, DateTime> _lastShotAt = new();

    public TankShooting(Canvas canvas, double cellSize, int mapW, int mapH)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _cellSize = cellSize;
        _mapW = mapW;
        _mapH = mapH;

        CompositionTarget.Rendering += OnUpdate;
    }
    
    public bool Shoot(UIElement tankVisual)
    {
        if (tankVisual == null) return false;

        if (_lastShotAt.TryGetValue(tankVisual, out var t) && (DateTime.Now - t).TotalSeconds < FireCooldownSecondsPerTank)
            return false;

        var state = TankRegistry.Tanks.FirstOrDefault(x => x.Visual == tankVisual);
        if (state == null) return false;

        var cx = Canvas.GetLeft(tankVisual) + tankVisual.RenderSize.Width / 2.0;
        var cy = Canvas.GetTop(tankVisual) + tankVisual.RenderSize.Height / 2.0;

        var angleDeg = state.Angle;
        var rad = (angleDeg - 90.0) * Math.PI / 180.0;

        var speed = BulletSpeedCellsPerSec * _cellSize;
        var thickness = Math.Max(1.0, 0.1 * _cellSize);

        var muzzleOffset = Math.Max(state.Width, state.Height) / 2.0 + thickness;
        var startX = cx + Math.Cos(rad) * muzzleOffset;
        var startY = cy + Math.Sin(rad) * muzzleOffset;

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
            Vx = Math.Cos(rad) * speed,
            Vy = Math.Sin(rad) * speed,
            Radius = thickness / 2.0,
            SpawnedAt = DateTime.Now,
            Owner = tankVisual
        };

        _bullets.Add(b);
        _lastShotAt[tankVisual] = DateTime.Now;
        return true;
    }

    private void OnUpdate(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var dt = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        if (dt <= 0) return;

        var remove = new List<Bullet>();

        foreach (var b in _bullets)
        {
            b.X += b.Vx * dt;
            b.Y += b.Vy * dt;

            Canvas.SetLeft(b.Visual, b.X - b.Radius);
            Canvas.SetTop(b.Visual, b.Y - b.Radius);

            if ((now - b.SpawnedAt).TotalSeconds > BulletLifetimeSeconds)
            {
                remove.Add(b);
                continue;
            }

            if (b.X < -20 || b.Y < -20 || b.X > _mapW * _cellSize + 20 || b.Y > _mapH * _cellSize + 20)
            {
                remove.Add(b);
                continue;
            }

            foreach (var t in TankRegistry.Tanks)
            {
                if (t.Visual == b.Owner) continue;

                var dx = t.X - b.X;
                var dy = t.Y - b.Y;
                var distSq = dx * dx + dy * dy;

                var tankRadius = Math.Max(t.Width, t.Height) / 2.0;

                if (!(distSq <=
                      (tankRadius + b.Radius) * (tankRadius + b.Radius)))
                    continue;
                remove.Add(b);
                break;
            }
        }

        foreach (var b in remove)
        {
            _canvas.Children.Remove(b.Visual);
            _bullets.Remove(b);
        }
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
    }
}
