using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Logic;

namespace Client.UI.Game;

public partial class Game
{
    private List<TankSpawner.SpawnedTank> _tanks = [];
    private readonly List<TankController> _tankControllers = [];
    private readonly HashSet<(int, int, int, int)> _passageSet = [];
    private readonly TankSpawner _tankSpawner = new();

    private void InitializeTanks(Window window)
    {
        if (_cellSize <= 0) return;

        var w = (int)(MazeCanvas.Width / _cellSize);
        var h = (int)(MazeCanvas.Height / _cellSize);

        _passageSet.Clear();
        foreach (var p in _passages)
        {
            _passageSet.Add((p.x1, p.y1, p.x2, p.y2));
            _passageSet.Add((p.x2, p.y2, p.x1, p.y1));
        }

        _tanks = _tankSpawner.Spawn(
            MazeCanvas,
            _playerCount,
            w,
            h,
            _cellSize
        );

        TankRegistry.Tanks.Clear();
        foreach (var spawned in _tanks)
        {
            TankRegistry.Tanks.Add(new TankState
            {
                Visual = spawned.Tank,
                Width = spawned.Tank.Width,
                Height = spawned.Tank.Height,
                X = Canvas.GetLeft(spawned.Tank) + spawned.Tank.Width / 2,
                Y = Canvas.GetTop(spawned.Tank) + spawned.Tank.Height / 2,
                Angle = 0
            });
        }

        if (_tanks.Count >= 1)
        {
            _tankControllers.Add(new TankController(
                _tanks[0].Tank, window, 
                Key.W, Key.S, Key.A, Key.D, 
                0, _cellSize, _passageSet, w, h));
        }

        if (_tanks.Count >= 2)
        {
            _tankControllers.Add(new TankController(
                _tanks[1].Tank,
                MazeCanvas,
                0,
                _cellSize,
                _passageSet,
                w, h));
        }

        if (_tanks.Count >= 3)
        {
            _tankControllers.Add(new TankController(
                _tanks[3].Tank, window, 
                Key.O, Key.L, Key.K, Key.OemSemicolon, 
                0, _cellSize, _passageSet, w, h));
        }

        if (_tanks.Count >= 4)
        {
            _tankControllers.Add(new TankController(
                _tanks[2].Tank, window, 
                Key.NumPad8, Key.NumPad5, Key.NumPad4, Key.NumPad6, 
                0, _cellSize, _passageSet, w, h));
        }

        GC.KeepAlive(_tankControllers);
    }
}
