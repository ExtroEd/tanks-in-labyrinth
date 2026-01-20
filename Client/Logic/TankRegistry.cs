using System.Windows;

namespace Client.Logic;

public class TankState
{
    public int PlayerIndex { get; init; }

    public bool IsAlive { get; set; } = true;

    public int Kills { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Angle { get; set; }
    public double Width { get; init; }
    public double Height { get; init; }
    public required UIElement Visual { get; init; }
}

public static class TankRegistry
{
    public static readonly List<TankState> Tanks = [];

    public static void UpdateState(UIElement visual, double x, double y, double angle)
    {
        var state = Tanks.FirstOrDefault(t => t.Visual == visual);
        if (state == null) return;
        if (!state.IsAlive) return;
        state.X = x;
        state.Y = y;
        state.Angle = angle;
    }
}
