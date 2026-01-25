using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.Logic;

namespace Client.UI.Game;

public partial class Game
{
    private List<TankSpawner.SpawnedTank> _tanks = [];
    private readonly HashSet<(int, int, int, int)> _passageSet = [];
    private readonly TankSpawner _tankSpawner = new();

    private TankShooting? _tankShooting;
    
    private UIElement? _shootTargetP1;
    private UIElement? _shootTargetP2;
    private UIElement? _shootTargetP3;
    private UIElement? _shootTargetP4;

    private void InitializeTanks(Window window)
    {
        if (_cellSize <= 0) return;
        
        window.PreviewKeyDown -= OnWindowKeyDown; 
        MazeCanvas.MouseLeftButtonDown -= OnCanvasMouseDown;
        
        var w = (int)(MazeCanvas.Width / _cellSize);
        var h = (int)(MazeCanvas.Height / _cellSize);

        foreach (var c in _controllers) c.Dispose();
        _controllers.Clear();
        
        _tanks.Clear();
        
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
        for (var idx = 0; idx < _tanks.Count; idx++)
        {
            var spawned = _tanks[idx];
            Panel.SetZIndex(spawned.Tank, 2000);

            TankRegistry.Tanks.Add(new TankState
            {
                PlayerIndex = idx,
                IsAlive = true,
                Kills = 0,

                Visual = spawned.Tank,
                Width = spawned.Tank.Width,
                Height = spawned.Tank.Height,
                X = Canvas.GetLeft(spawned.Tank) + spawned.Tank.Width / 2,
                Y = Canvas.GetTop(spawned.Tank) + spawned.Tank.Height / 2,
                Angle = 0
            });
        }
        
        _shootTargetP1 = _tanks.Count >= 1 ? _tanks[0].Tank : null;
        _shootTargetP2 = _tanks.Count >= 2 ? _tanks[1].Tank : null;
        _shootTargetP3 = _tanks.Count >= 3 ? _tanks[2].Tank : null;
        _shootTargetP4 = _tanks.Count >= 4 ? _tanks[3].Tank : null;

        _tankShooting?.Dispose();
        _tankShooting = new TankShooting(MazeCanvas, _cellSize, w, h, _passageSet, _roundManager!);
        _tankShooting.TankHit += HandleTankHit;
        
        if (_tanks.Count >= 1)
        {
            _controllers.Add(new TankController(
                _tanks[0].Tank, window, 
                Key.W, Key.S, Key.A, Key.D, 
                0, _cellSize, _passageSet, w, h));
        }

        if (_tanks.Count >= 2)
        {
            _controllers.Add(new TankController(
                window,
                _tanks[1].Tank,
                MazeCanvas,
                0,
                _cellSize,
                _passageSet,
                w, h));
        }

        if (_tanks.Count >= 3)
        {
            _controllers.Add(new TankController(
                _tanks[3].Tank, window, 
                Key.O, Key.L, Key.K, Key.OemSemicolon, 
                0, _cellSize, _passageSet, w, h));
        } // Good

        if (_tanks.Count >= 4)
        {
            _controllers.Add(new TankController(
                _tanks[2].Tank, window, 
                Key.NumPad8, Key.NumPad5, Key.NumPad4, Key.NumPad6, 
                0, _cellSize, _passageSet, w, h));
        } // Good

        window.PreviewKeyDown += OnWindowKeyDown;

        MazeCanvas.MouseLeftButtonDown += OnCanvasMouseDown;

        GC.KeepAlive(_controllers);
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (_tankShooting == null) return;
        switch (e.Key)
        {
            case Key.OemTilde: _tankShooting.Shoot(_shootTargetP1); break;
            case Key.Add: _tankShooting.Shoot(_shootTargetP3); break;
            case Key.OemQuotes: _tankShooting.Shoot(_shootTargetP4); break;
        }
    }
    
    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        _tankShooting?.Shoot(_shootTargetP2);
    }
    
    private void HandleTankHit(TankState hitTank, UIElement? owner)
    {
        if (owner != null)
        {
            var killer = TankRegistry.Tanks.FirstOrDefault(t => t.Visual == owner);
            if (killer != null)
            {
                var killerIdx = killer.PlayerIndex;

                TankRegistry.SessionScores.TryAdd(killerIdx, 0);
                TankRegistry.SessionScores[killerIdx]++;
            }
        }

        if (MazeCanvas.Children.Contains(hitTank.Visual))
        {
            MazeCanvas.Children.Remove(hitTank.Visual);
        }

        hitTank.IsAlive = false;

        var spawned = _tanks.FirstOrDefault(s => s.Tank == hitTank.Visual);
        if (spawned != null)
        {
            _tanks.Remove(spawned);
        }
        
        if (_shootTargetP1 == hitTank.Visual) _shootTargetP1 = null;
        if (_shootTargetP2 == hitTank.Visual) _shootTargetP2 = null;
        if (_shootTargetP3 == hitTank.Visual) _shootTargetP3 = null;
        if (_shootTargetP4 == hitTank.Visual) _shootTargetP4 = null;
        
        Hud.RefreshScores();
    }
}
