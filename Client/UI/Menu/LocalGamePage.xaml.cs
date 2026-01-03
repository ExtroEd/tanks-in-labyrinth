using System.Windows;

namespace Client.UI.Menu;

public partial class LocalGamePage
{
    public LocalGamePage()
    {
        InitializeComponent();
    }

    private void OnePlayer_Click(object sender, RoutedEventArgs e) => OpenLobby(1);
    private void TwoPlayers_Click(object sender, RoutedEventArgs e) => OpenLobby(2);
    private void ThreePlayers_Click(object sender, RoutedEventArgs e) => OpenLobby(3);
    private void FourPlayers_Click(object sender, RoutedEventArgs e) => OpenLobby(4);

    private static void OpenLobby(int players)
    {
        if (Application.Current.MainWindow is MainWindow main)
        {
            main.SwitchContent(new LocalPlayersPage(players));
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
