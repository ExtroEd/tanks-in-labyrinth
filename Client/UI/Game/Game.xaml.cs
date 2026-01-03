using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Client.Assets;
using Client.Logic;

namespace Client.UI.Game;

public partial class Game
{
    private double _cellSize;
    private double _currentWallThickness;
    private readonly Random _random = new();
    private readonly int _playerCount;

    public Game(int playerCount, string[] names) {
        _playerCount = playerCount;
        InitializeComponent();
        Hud.Initialize(playerCount, names);
        GenerateAndDrawMaze();
    }

    private void GenerateAndDrawMaze()
    {
        var widthCells = _random.Next(8, 31);
        var heightCells = widthCells / 2;

        var maxWidth = SystemParameters.PrimaryScreenWidth * 0.85;
        var maxHeight = SystemParameters.PrimaryScreenHeight * 0.85;

        var cellSizeX = maxWidth / widthCells;
        var cellSizeY = maxHeight / heightCells;
        _cellSize = Math.Min(cellSizeX, cellSizeY);
        _currentWallThickness = _cellSize * 0.08;

        var generator = new LabyrinthGenerator(widthCells, heightCells);
        var passages = generator.Generate();

        DrawMaze(widthCells, heightCells, passages);
        SpawnTanks(widthCells, heightCells);
    }

    private void SpawnTanks(int w, int h)
    {
        var shuffledCells = Enumerable.Range(0, w)
            .SelectMany(x => Enumerable.Range(0, h).Select(y => (x, y)))
            .OrderBy(_ => _random.Next()).ToList();

        Color[] playerColors = [Colors.Green, Colors.Red, Colors.Blue, Colors.Yellow];
        Color[] turretColors = [Colors.Green, Colors.Red, Colors.Blue, Colors.Yellow];

        var targetSize = _cellSize * 0.6;

        for (var i = 0; i < _playerCount; i++)
        {
            var cell = shuffledCells[i];
            
            var tank = new NormalTank(targetSize);

            tank.SetColor(playerColors[i], turretColors[i]);
            
            var xPos = (cell.x * _cellSize) + (_cellSize - targetSize) / 2.0;
            var yPos = (cell.y * _cellSize) + (_cellSize - targetSize) / 2.0;

            Canvas.SetLeft(tank, xPos);
            Canvas.SetTop(tank, yPos);
            MazeCanvas.Children.Add(tank);

            tank.Rotate(_random.Next(0, 360));
        }
    }

    private void DrawMaze(int w, int h, List<(int x1, int y1, int x2, int y2)> passages)
    {
        MazeCanvas.Children.Clear();
        MazeCanvas.Width = w * _cellSize;
        MazeCanvas.Height = h * _cellSize;
        MazeCanvas.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));

        var wallBrush = new SolidColorBrush(Color.FromRgb(82, 82, 82));
        var passageSet = new HashSet<(int, int, int, int)>();
        foreach (var p in passages)
        {
            passageSet.Add((p.x1, p.y1, p.x2, p.y2));
            passageSet.Add((p.x2, p.y2, p.x1, p.y1));
        }

        for (var x = 0; x <= w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (!(x > 0 && x < w && passageSet.Contains((x - 1, y, x, y))))
                {
                    MazeCanvas.Children.Add(new System.Windows.Shapes.Line
                    {
                        X1 = x * _cellSize, Y1 = y * _cellSize,
                        X2 = x * _cellSize, Y2 = (y + 1) * _cellSize,
                        Stroke = wallBrush, StrokeThickness = _currentWallThickness,
                        StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
                    });
                }
            }
        }

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y <= h; y++)
            {
                if (!(y > 0 && y < h && passageSet.Contains((x, y - 1, x, y))))
                {
                    MazeCanvas.Children.Add(new System.Windows.Shapes.Line
                    {
                        X1 = x * _cellSize, Y1 = y * _cellSize,
                        X2 = (x + 1) * _cellSize, Y2 = y * _cellSize,
                        Stroke = wallBrush, StrokeThickness = _currentWallThickness,
                        StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
                    });
                }
            }
        }
    }
}
