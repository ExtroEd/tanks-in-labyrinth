using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Client.Logic;

namespace Client.UI.Game;

public partial class Game
{
    private RoundManager? _roundManager;
    private TankShooting? _shooting;

    private double _cellSize;
    private double _currentWallThickness;
    private readonly Random _random = new();
    private readonly int _playerCount;
    private List<(int x1, int y1, int x2, int y2)> _passages = [];

    private static readonly Brush LightGray =
        new SolidColorBrush(Color.FromRgb(222, 222, 222));

    private static readonly Brush Gray =
        new SolidColorBrush(Color.FromRgb(212, 212, 212));

    public Game(int playerCount, string[] names)
    {
        _playerCount = playerCount;
        InitializeComponent();
        Hud.Initialize(playerCount, names);

        Loaded += OnLoaded;
    }

    private static double SmoothStep(double t)
    {
        return t * t * (3 - 2 * t);
    }

    private static double Hash(int x, int y)
    {
        unchecked
        {
            var h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return (h & 0x7fffffff) / (double)int.MaxValue;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        GenerateAndDrawMaze();

        var window = Window.GetWindow(this);
        if (window == null) return;

        _roundManager = new RoundManager(ResetRound);

        InitializeTanks(window);
        
        var uri = new Uri("pack://application:,,,/Client;component/Assets/Cursors/precision.cur", UriKind.Absolute);
        var res = Application.GetResourceStream(uri);
        if (res?.Stream == null) return;
        using var s = res.Stream;
        var cursor = new Cursor(s);
        Mouse.OverrideCursor = cursor;
    }

    private void GenerateAndDrawMaze()
    {
        var widthCells = _random.Next(6, 7);
        var heightCells = widthCells / 2;

        var maxWidth = SystemParameters.PrimaryScreenWidth * 0.85;
        var maxHeight = SystemParameters.PrimaryScreenHeight * 0.85;

        var cellSizeX = maxWidth / widthCells;
        var cellSizeY = maxHeight / heightCells;
        _cellSize = Math.Min(cellSizeX, cellSizeY);
        _currentWallThickness = _cellSize * 0.08;

        var generator = new LabyrinthGenerator(widthCells, heightCells);
        var passages = generator.Generate();

        _passages = passages;

        DrawMaze(widthCells, heightCells, passages);
    }

    private void DrawMaze(int w, int h, List<(int x1, int y1, int x2, int y2)> passages)
    {
        MazeCanvas.Children.Clear();
        MazeCanvas.Width = w * _cellSize;
        MazeCanvas.Height = h * _cellSize;

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var noiseScale = Math.Max(w, h) / 12.0;
                
                var fx = x / noiseScale;
                var fy = y / noiseScale;

                var x0 = (int)Math.Floor(fx);
                var y0 = (int)Math.Floor(fy);
                var x1 = x0 + 1;
                var y1 = y0 + 1;

                var sx = SmoothStep(fx - x0);
                var sy = SmoothStep(fy - y0);

                var n00 = Hash(x0, y0);
                var n10 = Hash(x1, y0);
                var n01 = Hash(x0, y1);
                var n11 = Hash(x1, y1);

                var nx0 = n00 + (n10 - n00) * sx;
                var nx1 = n01 + (n11 - n01) * sx;
                var noise = nx0 + (nx1 - nx0) * sy;

                var brush = noise < 0.5 ? LightGray : Gray;
                
                const double overlap = 1.0;

                MazeCanvas.Children.Add(new System.Windows.Shapes.Rectangle
                {
                    Width = _cellSize + overlap,
                    Height = _cellSize + overlap,
                    Fill = brush,
                    SnapsToDevicePixels = true
                });

                Canvas.SetLeft(MazeCanvas.Children[^1], x * _cellSize);
                Canvas.SetTop (MazeCanvas.Children[^1], y * _cellSize);
            }
        }

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
                        Stroke = wallBrush,
                        StrokeThickness = _currentWallThickness,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
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
                        Stroke = wallBrush,
                        StrokeThickness = _currentWallThickness,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    });
                }
            }
        }
    }
    
    private void ResetRound()
    {
        _roundManager?.StopTimer();

        _shooting?.Cleanup(); 

        TankRegistry.Tanks.Clear();
    
        GenerateAndDrawMaze();

        var window = Window.GetWindow(this);
        if (window != null)
        {
            InitializeTanks(window);
        }
    }
}
