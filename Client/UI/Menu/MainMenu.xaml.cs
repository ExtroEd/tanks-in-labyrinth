using System.Windows;

namespace Client.UI.Menu;

public partial class MainMenu
{
    public MainMenu()
    {
        InitializeComponent();
    }

    private void LocalGame_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is MainWindow main)
        {
            main.SwitchContent(new LocalGamePage());
        }
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
