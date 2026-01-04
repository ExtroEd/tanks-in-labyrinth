using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Logic;

public class TankController
{
    private readonly UIElement _tank;
    private readonly double _cellSize;
    private readonly HashSet<(int, int, int, int)> _passages;
    private readonly int _mapW;
    private readonly int _mapH;

    private double _angle;
    private static readonly Random Random = new();

    private const double SpeedCells = 1.5;
    private const double RotationSpeed = 250;

    private bool _forward, _backward, _left, _right;
    private DateTime _lastUpdate = DateTime.Now;

    public TankController(
        UIElement tank,
        Window window,
        Key forward, Key backward, Key left, Key right,
        double visualAngle,
        double cellSize,
        HashSet<(int, int, int, int)> passages,
        int mapW, int mapH)
    {
        _tank = tank;
        _cellSize = cellSize;
        _passages = passages;
        _mapW = mapW;
        _mapH = mapH;

        _angle = (Random.NextDouble() * 360.0 + visualAngle) % 360;

        CompositionTarget.Rendering += OnUpdate;

        window.PreviewKeyDown += (_, e) => HandleKey(e.Key, true, forward, backward, left, right);
        window.PreviewKeyUp += (_, e) => HandleKey(e.Key, false, forward, backward, left, right);
    }

    private void HandleKey(Key key, bool state, Key f, Key b, Key l, Key r)
    {
        if (key == f) _forward = state;
        if (key == b) _backward = state;
        if (key == l) _left = state;
        if (key == r) _right = state;
    }

    private void OnUpdate(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var delta = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        var curX = Canvas.GetLeft(_tank) + _tank.RenderSize.Width / 2;
        var curY = Canvas.GetTop(_tank) + _tank.RenderSize.Height / 2;

        TankRegistry.UpdateState(_tank, curX, curY, _angle);
        
        if (_left || _right)
        {
            double rotationDir = _left ? -1 : 1;
            var nextAngle = (_angle + rotationDir * RotationSpeed * delta) % 360;

            var wallHit = TankWallCollision.IsCollidingWithWall(curX, curY, nextAngle, _tank.RenderSize.Width, _tank.RenderSize.Height, _cellSize, _passages, _mapW, _mapH);
            var tankHit = TankTankCollision.IsHittingAnyTank(_tank, curX, curY, nextAngle, _tank.RenderSize.Width, _tank.RenderSize.Height);

            if (wallHit || tankHit)
            {
                var resolved = false;
                for (var r = 1; r <= 3; r++)
                {
                    for (var a = 0; a < 360; a += 45)
                    {
                        var testX = curX + Math.Cos(a * Math.PI / 180) * r;
                        var testY = curY + Math.Sin(a * Math.PI / 180) * r;

                        if (TankWallCollision.IsCollidingWithWall(testX, testY,
                                nextAngle, _tank.RenderSize.Width,
                                _tank.RenderSize.Height, _cellSize, _passages,
                                _mapW, _mapH) ||
                            TankTankCollision.IsHittingAnyTank(_tank, testX,
                                testY, nextAngle, _tank.RenderSize.Width,
                                _tank.RenderSize.Height)) continue;
                        curX = testX;
                        curY = testY;
                        _angle = nextAngle;
                        resolved = true;
                        break;
                    }
                    if (resolved) break;
                }
            }
            else
            {
                _angle = nextAngle;
            }
        }

        var move = 0.0;
        if (_forward) move += SpeedCells * _cellSize * delta;
        if (_backward) move -= SpeedCells * _cellSize * delta;

        if (Math.Abs(move) > 0.01)
        {
            var steps = (int)Math.Ceiling(Math.Abs(move) / 2.0);
            var stepMove = move / steps;
            var radMove = (_angle - 90) * Math.PI / 180;
            var vx = Math.Cos(radMove) * stepMove;
            var vy = Math.Sin(radMove) * stepMove;

            for (var i = 0; i < steps; i++)
            {
                var nextX = curX + vx;
                var nextY = curY + vy;

                var wallHit = TankWallCollision.IsCollidingWithWall(nextX, nextY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height, _cellSize, _passages, _mapW, _mapH);
                
                if (!wallHit)
                {
                    var hitTank = TankTankCollision.GetCollidingTank(_tank, nextX, nextY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height);
                    
                    if (hitTank != null)
                    {
                        if (!TankTankCollision.TryPush(hitTank, vx, vy, _cellSize, _passages, _mapW, _mapH, curX, curY)) continue;
                    }

                    curX = nextX;
                    curY = nextY;
                }
                else
                {
                    if (!TankWallCollision.IsCollidingWithWall(nextX, curY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height, _cellSize, _passages, _mapW, _mapH) &&
                        !TankTankCollision.IsHittingAnyTank(_tank, nextX, curY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height))
                    {
                        curX = nextX;
                    }
                    else if (!TankWallCollision.IsCollidingWithWall(curX, nextY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height, _cellSize, _passages, _mapW, _mapH) &&
                             !TankTankCollision.IsHittingAnyTank(_tank, curX, nextY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height))
                    {
                        curY = nextY;
                    }
                }
            }
        }

        Canvas.SetLeft(_tank, curX - _tank.RenderSize.Width / 2);
        Canvas.SetTop(_tank, curY - _tank.RenderSize.Height / 2);
        
        TankRegistry.UpdateState(_tank, curX, curY, _angle);

        _tank.RenderTransform = new RotateTransform(_angle, _tank.RenderSize.Width / 2, _tank.RenderSize.Height / 2);
    }
}
