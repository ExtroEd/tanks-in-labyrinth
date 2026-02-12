using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;

using Client.Logic;

namespace Client.UI.Game;

public class BoxesUIManager : IDisposable
{
    private readonly BoxesManager _manager;
    private readonly Canvas _canvas;
    private readonly Dictionary<BoxesManager.BoxInstance, UIElement> _visuals = [];
    private readonly DispatcherTimer _timer;
    private bool _disposed;

    public event Action<TankState, string>? BoxPicked; // (танк, тип)

    public BoxesUIManager(BoxesManager manager, Canvas canvas)
    {
        _manager = manager;
        _canvas = canvas;

        // При каждом спауне визуализируем ящик
        _manager.BoxSpawned += OnBoxSpawned;
        _manager.BoxRemoved += OnBoxRemoved;

        // Таймер: каждые 5 секунд спаун
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timer.Tick += (_, _) => _manager.TrySpawnBox();
        _timer.Start();

        // Проверка на подбор ящиков — раз в кадр
        CompositionTarget.Rendering += OnFrame;
    }

    private void OnBoxSpawned(BoxesManager.BoxInstance box)
    {
        // Загружаем класс из XAML по имени
        var type = Type.GetType($"Client.Assets.Boxes.{box.BoxType}");
        if (type == null) return;
        var visual = Activator.CreateInstance(type) as UIElement;
        if (visual is FrameworkElement fe)
        {
            fe.Width = fe.Height = _manager.Boxes.Count > 0 ? _manager.Boxes[0].Visual is FrameworkElement feSample ? feSample.Width : 0 : _canvas.ActualWidth * 0.08;
            fe.Width = fe.Height = _manager.Boxes.Count > 0 ? fe.Width : _canvas.ActualWidth * 0.08;
            fe.RenderTransformOrigin = new Point(0.5, 0.5);
            fe.RenderTransform = new RotateTransform(box.AngleDeg);
        }
        else if (visual is not null)
        {
            // Fallback
            visual.RenderTransform = new RotateTransform(box.AngleDeg);
        }
        // Размер
        double size = _manager.Boxes.Count > 0 && _manager.Boxes[0].Visual is FrameworkElement fePrev ?
            fePrev.Width : _canvas.ActualWidth * 0.8;
        if (visual is FrameworkElement fe2)
        {
            fe2.Width = fe2.Height = size;
            Canvas.SetLeft(fe2, box.CellX * _manager.CellSize + (_manager.CellSize - size) / 2);
            Canvas.SetTop(fe2, box.CellY * _manager.CellSize + (_manager.CellSize - size) / 2);
        }
        else
        {
            Canvas.SetLeft(visual, box.CellX * _manager.CellSize);
            Canvas.SetTop(visual, box.CellY * _manager.CellSize);
        }
        Panel.SetZIndex(visual, 900);
        _canvas.Children.Add(visual);
        _visuals[box] = visual;
        box.Visual = visual;
    }

    private void OnBoxRemoved(BoxesManager.BoxInstance box)
    {
        if (_visuals.TryGetValue(box, out var visual))
        {
            _canvas.Children.Remove(visual);
            _visuals.Remove(box);
        }
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (_disposed) return;
        var pick = _manager.CheckPick();
        if (pick != null)
        {
            _manager.RemoveBox(pick.Value.box);
            BoxPicked?.Invoke(pick.Value.tank, pick.Value.box.BoxType);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _timer.Stop();
        _timer.Tick -= (_, _) => _manager.TrySpawnBox();

        _manager.BoxSpawned -= OnBoxSpawned;
        _manager.BoxRemoved -= OnBoxRemoved;

        CompositionTarget.Rendering -= OnFrame;

        foreach (var visual in _visuals.Values)
            _canvas.Children.Remove(visual);

        _visuals.Clear();
    }
}
