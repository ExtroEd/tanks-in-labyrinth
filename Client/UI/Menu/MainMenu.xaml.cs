using System.Windows;

namespace Client.UI.Menu;

public partial class MainMenu
{
    public MainMenu()
    {
        InitializeComponent();
        Topmost = true;
    }

    private void LocalGame_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("In develop");
    }

    private void LanGame_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("In develop");
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
