using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Logic;

public sealed class TankController : IDisposable
{
    private readonly UIElement _tank;
    private readonly double _cellSize;
    private readonly HashSet<(int, int, int, int)> _passages;
    private readonly int _mapW;
    private readonly int _mapH;
    private readonly bool _isMouse;
    
    private Canvas? _canvas;
    private Window? _window;

    private double _angle;
    private static readonly Random Random = new();
    private DateTime _lastUpdate = DateTime.Now;
    private bool _disposed;

    private readonly Key _forwardKey, _backwardKey, _leftKey, _rightKey;
    private bool _forward, _backward, _left, _right;

    private const double SpeedCells = 1.5;
    private const double RotationSpeed = 250;
    private const double BackwardFactor = 0.75;
    private const double TimeStepLimit = 0.1;

    // --- Конструктор для КЛАВИАТУРЫ ---
    public TankController(
        UIElement tank, Window window,
        Key forward, Key backward, Key left, Key right,
        double visualAngle, double cellSize,
        HashSet<(int, int, int, int)> passages, int mapW, int mapH)
        : this(tank, window, visualAngle, cellSize, passages, mapW, mapH)
    {
        _forwardKey = forward;
        _backwardKey = backward;
        _leftKey = left;
        _rightKey = right;
        _isMouse = false;
        InitializeEvents();
    }

    // --- Конструктор для МЫШИ ---
    public TankController(
        Window window, UIElement tank, Canvas canvas,
        double visualAngle, double cellSize,
        HashSet<(int, int, int, int)> passages, int mapW, int mapH)
        : this(tank, window, visualAngle, cellSize, passages, mapW, mapH)
    {
        _canvas = canvas;
        _isMouse = true;
        InitializeEvents();
    }

    private TankController(UIElement tank, Window window, double visualAngle, double cellSize,
        HashSet<(int, int, int, int)> passages, int mapW, int mapH)
    {
        _tank = tank;
        _window = window;
        _cellSize = cellSize;
        _passages = passages;
        _mapW = mapW;
        _mapH = mapH;
        _angle = (Random.NextDouble() * 360.0 + visualAngle) % 360;
    }

    private void InitializeEvents()
    {
        CompositionTarget.Rendering += OnUpdate;
        if (_window == null) return;
        _window.PreviewKeyDown += HandleKeyDown;
        _window.PreviewKeyUp += HandleKeyUp;
    }
    
    private void HandleKeyDown(object sender, KeyEventArgs e) => HandleKey(e.Key, true);
    private void HandleKeyUp(object sender, KeyEventArgs e) => HandleKey(e.Key, false);

    private void HandleKey(Key key, bool state)
    {
        if (_isMouse) return;
        if (key == _forwardKey) _forward = state;
        if (key == _backwardKey) _backward = state;
        if (key == _leftKey) _left = state;
        if (key == _rightKey) _right = state;
    }
    
    private void OnUpdate(object? sender, EventArgs e)
    {
        if (_disposed) return;

        var myState = TankRegistry.Tanks.FirstOrDefault(t => t.Visual == _tank);
        if (myState is { IsAlive: false }) return;

        var now = DateTime.Now;
        var delta = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        if (delta > TimeStepLimit) delta = TimeStepLimit;

        var curX = Canvas.GetLeft(_tank) + _tank.RenderSize.Width / 2;
        var curY = Canvas.GetTop(_tank) + _tank.RenderSize.Height / 2;

        double moveAmount;
        double? targetRotationAngle;

        if (_isMouse && _canvas != null)
        {
            ProcessMouseInput(curX, curY, delta, out moveAmount, out targetRotationAngle);
        }
        else
        {
            ProcessKeyboardInput(delta, out moveAmount, out targetRotationAngle);
        }

        if (targetRotationAngle.HasValue)
        {
            TryApplyRotation(targetRotationAngle.Value, ref curX, ref curY);
        }

        if (Math.Abs(moveAmount) > 0.01)
        {
            ApplyMovement(moveAmount, ref curX, ref curY);
        }

        UpdateVisuals(curX, curY);
    }
    
    private void ProcessKeyboardInput(double delta, out double moveAmount, out double? targetAngle)
    {
        moveAmount = 0;
        targetAngle = null;

        if (_left || _right)
        {
            double dir = _left ? -1 : 1;
            targetAngle = (_angle + dir * RotationSpeed * delta) % 360;
        }

        if (_forward) moveAmount += SpeedCells * _cellSize * delta;
        if (_backward) moveAmount -= SpeedCells * BackwardFactor * _cellSize * delta;
    }

    private void ProcessMouseInput(double curX, double curY, double delta, out double moveAmount, out double? targetAngle)
    {
        moveAmount = 0;
        targetAngle = null;
        
        var mousePos = Mouse.GetPosition(_canvas);
        var dx = mousePos.X - curX;
        var dy = mousePos.Y - curY;
        var dist = Math.Sqrt(dx * dx + dy * dy);

        var desiredAngle = (Math.Atan2(dy, dx) * 180.0 / Math.PI + 90.0) % 360.0;
        if (desiredAngle < 0) desiredAngle += 360.0;

        var rotThreshold = 0.1 * _cellSize;
        var moveThreshold = 1.0 * _cellSize;
        
        if (dist > rotThreshold)
        {
            var diff = NormalizeAngleDiff(desiredAngle - _angle);
            if (Math.Abs(diff) > 0.5)
            {
                var dir = diff < 0 ? -1 : 1;
                var rotationStep = dir * RotationSpeed * delta;
                
                if (Math.Abs(rotationStep) > Math.Abs(diff)) 
                    targetAngle = desiredAngle;
                else 
                    targetAngle = (_angle + rotationStep) % 360;
            }
        }

        var isAtEdge = mousePos.X <= 0.5 || mousePos.Y <= 0.5 || 
                       mousePos.X >= Math.Max(_canvas!.ActualWidth, _canvas.Width) - 0.5 || 
                       mousePos.Y >= Math.Max(_canvas.ActualHeight, _canvas.Height) - 0.5;

        if (!(dist > moveThreshold) && !isAtEdge) return;
        var angleToCursor = NormalizeAngleDiff(desiredAngle - _angle);
        if (Math.Abs(angleToCursor) <= 90.0)
            moveAmount = SpeedCells * _cellSize * delta;
        else
            moveAmount = -SpeedCells * BackwardFactor * _cellSize * delta;
    }
    
    private void TryApplyRotation(double nextAngle, ref double curX, ref double curY)
    {
        if (nextAngle < 0) nextAngle += 360;

        if (!IsColliding(curX, curY, nextAngle))
        {
            _angle = nextAngle;
            return;
        }
        
        for (var r = 1; r <= 3; r++)
        {
            for (var a = 0; a < 360; a += 45)
            {
                var rad = a * Math.PI / 180;
                var testX = curX + Math.Cos(rad) * r;
                var testY = curY + Math.Sin(rad) * r;

                if (IsColliding(testX, testY, nextAngle)) continue;
                curX = testX;
                curY = testY;
                _angle = nextAngle;
                return;
            }
        }
    }

    private void ApplyMovement(double move, ref double curX, ref double curY)
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

            if (!IsColliding(nextX, nextY, _angle))
            {
                var hitTank = TankTankCollision.GetCollidingTank(_tank, nextX, nextY, _angle, _tank.RenderSize.Width, _tank.RenderSize.Height);
                if (hitTank != null)
                {
                   if (!TankTankCollision.TryPush(hitTank, vx, vy, _cellSize, _passages, _mapW, _mapH, curX, curY)) 
                       continue;
                }

                curX = nextX;
                curY = nextY;
            }
            else
            {
                if (!IsColliding(nextX, curY, _angle))
                {
                    curX = nextX;
                }
                else if (!IsColliding(curX, nextY, _angle))
                {
                    curY = nextY;
                }
            }
        }
    }

    private bool IsColliding(double x, double y, double angle)
    {
        return TankWallCollision.IsCollidingWithWall(x, y, angle, _tank.RenderSize.Width, _tank.RenderSize.Height, _cellSize, _passages, _mapW, _mapH) ||
               TankTankCollision.IsHittingAnyTank(_tank, x, y, angle, _tank.RenderSize.Width, _tank.RenderSize.Height);
    }
    
    private void UpdateVisuals(double x, double y)
    {
        Canvas.SetLeft(_tank, x - _tank.RenderSize.Width / 2);
        Canvas.SetTop(_tank, y - _tank.RenderSize.Height / 2);
        
        _tank.RenderTransform = new RotateTransform(_angle, _tank.RenderSize.Width / 2, _tank.RenderSize.Height / 2);
        
        TankRegistry.UpdateState(_tank, x, y, _angle);
    }

    private static double NormalizeAngleDiff(double a)
    {
        while (a > 180) a -= 360;
        while (a < -180) a += 360;
        return a;
    }

    public void Dispose()
    {
        Dispose(true);
        
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            CompositionTarget.Rendering -= OnUpdate;
            if (_window != null)
            {
                _window.PreviewKeyDown -= HandleKeyDown;
                _window.PreviewKeyUp -= HandleKeyUp;
            }
        
            _canvas = null;
            _window = null;
        }

        _disposed = true;
    }

    ~TankController()
    {
        Dispose(false);
    }
}
