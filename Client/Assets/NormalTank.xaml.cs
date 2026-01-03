using System.Windows.Media;

namespace Client.Assets;

public partial class NormalTank
{
    public NormalTank()
    {
        InitializeComponent();
    }

    public NormalTank(double size) : this()
    {
        Width = size;
        Height = size;
    }

    public void SetColor(Color primary, Color secondary)
    {
        BodyArmor.Fill = new SolidColorBrush(primary);
        Turret.Fill = new SolidColorBrush(secondary);
        GunBarrel.Fill = new SolidColorBrush(secondary);
    }

    public void Rotate(double angle)
    {
        TankRotation.Angle = angle;
    }
}
