using System.Windows;

namespace Client.UI.Menu;

public partial class LocalGamePage
{
    public LocalGamePage()
    {
        InitializeComponent();
    }
    
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
