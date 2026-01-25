using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Client.Logic;

namespace Client.UI.Game;

public partial class GameScore
{
    public GameScore()
    {
        InitializeComponent();
    }

    public void Initialize(int playerCount, string[] names)
    {
        DetachPanels();
        ScoreGrid.Children.Clear();
        ScoreGrid.Columns = playerCount;

        if (playerCount >= 1) SetupPanel(P1Score, NameP1, IconP1, names[0], Colors.Green);
        if (playerCount >= 2) SetupPanel(P2Score, NameP2, IconP2, names[1], Colors.Red);
        if (playerCount >= 3) SetupPanel(P3Score, NameP3, IconP3, names[2], Colors.Blue);
        if (playerCount >= 4) SetupPanel(P4Score, NameP4, IconP4, names[3], Colors.Yellow);

        switch (playerCount)
        {
            case 1:
                AddPanel(P1Score);
                break;
            case 2:
                AddPanel(P1Score);
                AddPanel(P2Score);
                break;
            case 3:
                AddPanel(P1Score);
                AddPanel(P3Score);
                AddPanel(P2Score);
                break;
            case 4:
                AddPanel(P1Score);
                AddPanel(P4Score);
                AddPanel(P3Score);
                AddPanel(P2Score);
                break;
        }

        RefreshScores();
    }

    private static void SetupPanel(StackPanel panel, TextBlock nameLabel, Assets.NormalTank icon, string name, Color color)
    {
        panel.Visibility = Visibility.Visible;
        nameLabel.Text = name;
        icon.SetColor(color, color);
    }

    private void AddPanel(StackPanel panel)
    {
        ScoreGrid.Children.Add(panel);
    }

    private void DetachPanels()
    {
        StackPanel[] panels = [P1Score, P2Score, P3Score, P4Score];
        foreach (var panel in panels)
        {
            if (VisualTreeHelper.GetParent(panel) is Panel parent)
            {
                parent.Children.Remove(panel);
            }
        }
    }
    
    public void RefreshScores()
    {
        var sessionKills = TankRegistry.SessionScores;
        var sessionWins = TankRegistry.SessionWins;

        UpdatePlayerStats(KillsP1, WinsP1, 0);
        UpdatePlayerStats(KillsP2, WinsP2, 1);
        UpdatePlayerStats(KillsP3, WinsP3, 2);
        UpdatePlayerStats(KillsP4, WinsP4, 3);
        return;

        void UpdatePlayerStats(TextBlock? killsTb, TextBlock? winsTb, int playerIndex)
        {
            if (killsTb != null)
            {
                sessionKills.TryGetValue(playerIndex, out var kills);
                killsTb.Dispatcher.Invoke(() => killsTb.Text = $"Kills: {kills}");
            }

            if (winsTb == null) return;
            sessionWins.TryGetValue(playerIndex, out var wins);
            winsTb.Dispatcher.Invoke(() => winsTb.Text = $"Wins: {wins}");
        }
    }
}
