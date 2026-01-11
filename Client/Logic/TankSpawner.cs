using System.Windows.Controls;
using System.Windows.Media;
using Client.Assets;

namespace Client.Logic;

public class TankSpawner
{
    private readonly Random _random = new();

    public class SpawnedTank(NormalTank tank)
    {
        public NormalTank Tank { get; } = tank;
    }

    public List<SpawnedTank> Spawn(
        Canvas canvas,
        int playerCount,
        int w,
        int h,
        double cellSize)
    {
        var tanks = new List<SpawnedTank>();

        var shuffledCells = Enumerable.Range(0, w)
            .SelectMany(x => Enumerable.Range(0, h).Select(y => (x, y)))
            .OrderBy(_ => _random.Next())
            .ToList();

        Color[] colors = new[] { Colors.Green, Colors.Red, Colors.Blue, Colors.Yellow };
        var targetSize = cellSize * 0.65;

        for (var i = 0; i < playerCount; i++)
        {
            var cell = shuffledCells[i];
            var tank = new NormalTank(targetSize);
            tank.SetColor(colors[i], colors[i]);

            Canvas.SetLeft(tank, cell.x * cellSize + (cellSize - targetSize) / 2);
            Canvas.SetTop(tank, cell.y * cellSize + (cellSize - targetSize) / 2);

            canvas.Children.Add(tank);

            // Зарегистрируем состояние танка в реестре, чтобы столкновения/стрельба работали.
            TankRegistry.Tanks.Add(new TankState
            {
                Visual = tank,
                X = cell.x * cellSize + cellSize / 2.0,
                Y = cell.y * cellSize + cellSize / 2.0,
                Angle = 0,
                Width = targetSize,
                Height = targetSize
            });

            tanks.Add(new SpawnedTank(tank));
        }

        return tanks;
    }
}
