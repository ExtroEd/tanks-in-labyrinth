using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.Logic;

/// <summary>
/// Управляет спауном и жизнью ящиков на карте
/// </summary>
public class BoxesManager : IDisposable
{
    public class BoxInstance
    {
        public string BoxType = "";
        public int CellX, CellY;
        public double AngleDeg;
        public UIElement Visual = null!;
    }

    private readonly double _cellSize;
    private readonly int _mapW, _mapH;
    private readonly Random _random = new();
    private readonly List<BoxInstance> _boxes = [];
    public double CellSize => _cellSize;

    public IReadOnlyList<BoxInstance> Boxes => _boxes.AsReadOnly();

    public static readonly string[] BoxNames = [
        "Destroyer", "HighExplosive", "Laser", "MachineGun",
        "Mine", "MissileDefense", "ProtectiveField", "QuadraLaser",
        "Railgun", "Reducer", "Rocket", "Shotgun", "TotemOfUndying"
    ];

    public event Action<BoxInstance>? BoxSpawned;
    public event Action<BoxInstance>? BoxRemoved;

    public BoxesManager(double cellSize, int mapW, int mapH)
    {
        _cellSize = cellSize;
        _mapW = mapW;
        _mapH = mapH;
    }

    /// <summary>
    /// Пытается заспаунить новый ящик. Возвращает ящик или null
    /// </summary>
    public BoxInstance? TrySpawnBox()
    {
        var occupied = _boxes.Select(b => (b.CellX, b.CellY)).ToHashSet();
        var freeCells = Enumerable.Range(0, _mapW)
            .SelectMany(x => Enumerable.Range(0, _mapH).Select(y => (x, y)))
            .Where(c => !occupied.Contains(c))
            .ToList();

        if (freeCells.Count == 0) return null;

        var (cellX, cellY) = freeCells[_random.Next(freeCells.Count)];
        var boxTypeIndex = _random.Next(BoxNames.Length);
        var boxType = BoxNames[boxTypeIndex];
        var angle = _random.NextDouble() * 360.0;

        var box = new BoxInstance
        {
            BoxType = boxType,
            CellX = cellX,
            CellY = cellY,
            AngleDeg = angle
        };
        _boxes.Add(box);
        BoxSpawned?.Invoke(box);
        return box;
    }

    public void RemoveBox(BoxInstance box)
    {
        if (_boxes.Remove(box))
        {
            BoxRemoved?.Invoke(box);
        }
    }

    /// <summary>
    /// Проверить, есть ли танк в ячейке с ящиком. Возвращает найденную пару (tank, box)
    /// </summary>
    public (TankState tank, BoxInstance box)? CheckPick()
    {
        foreach (var tank in TankRegistry.Tanks)
        {
            if (!tank.IsAlive) continue;
            var tx = tank.X;
            var ty = tank.Y;
            foreach (var box in _boxes)
            {
                // Проверяем попадание центра танка в квадрат ящика
                var bx = box.CellX * _cellSize;
                var by = box.CellY * _cellSize;
                var size = _cellSize * 0.8;
                if (tx > bx && tx < bx + size && ty > by && ty < by + size)
                {
                    return (tank, box);
                }
            }
        }
        return null;
    }

    public void ClearAll()
    {
        _boxes.Clear();
    }

    public void Dispose()
    {
        ClearAll();
        BoxSpawned = null;
        BoxRemoved = null;
    }
}
