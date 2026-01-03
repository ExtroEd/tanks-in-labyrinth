using System.Windows;

namespace Client.UI.Menu;

public partial class LocalGamePage
{
    public LocalGamePage()
    {
        InitializeComponent();
    }

    private void OnePlayer_Click(object sender, RoutedEventArgs e)
    {
        StartGame(players: 1);
    }

    private void TwoPlayers_Click(object sender, RoutedEventArgs e)
    {
        StartGame(players: 2);
    }

    private void ThreePlayers_Click(object sender, RoutedEventArgs e)
    {
        StartGame(players: 3);
    }

    private void FourPlayers_Click(object sender, RoutedEventArgs e)
    {
        StartGame(players: 4);
    }

    private void StartGame(int players)
    {
        if (Application.Current.MainWindow is MainWindow main)
        {
            main.SwitchContent(new Game.Game(players));
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is MainWindow main)
        {
            main.SwitchContent(new MainMenu());
        }
    }
}
