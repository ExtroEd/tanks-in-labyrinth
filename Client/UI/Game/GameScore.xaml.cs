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

        // Создаём словарь панелей по PlayerIndex для корректной привязки к танкам
        var panelsByIndex = new Dictionary<int, (TextBlock? kills, TextBlock? suicides, TextBlock? wins)>
        {
            { 0, (KillsP1, SuicidesP1, WinsP1) },
            { 1, (KillsP2, SuicidesP2, WinsP2) },
            { 2, (KillsP3, SuicidesP3, WinsP3) },
            { 3, (KillsP4, SuicidesP4, WinsP4) }
        };

        // Обновляем только активные танки по их PlayerIndex
        foreach (var tank in TankRegistry.Tanks)
        {
            if (panelsByIndex.TryGetValue(tank.PlayerIndex, out var panels))
            {
                UpdatePlayerStats(panels.kills, panels.suicides, panels.wins, tank.PlayerIndex);
            }
        }

        void UpdatePlayerStats(TextBlock? killsTb, TextBlock? suicidesTb, TextBlock? winsTb, int playerIndex)
        {
            if (killsTb != null)
            {
                sessionKills.TryGetValue(playerIndex, out var kills);
                killsTb.Dispatcher.Invoke(() => killsTb.Text = $"Kills: {kills}");
            }

            if (suicidesTb != null)
            {
                var tank = TankRegistry.Tanks.FirstOrDefault(t => t.PlayerIndex == playerIndex);
                var suicides = tank?.Suicides ?? TankRegistry.PersistentSuicides.GetValueOrDefault(playerIndex, 0);
                suicidesTb.Dispatcher.Invoke(() => suicidesTb.Text = $"Suicides: {suicides}");
            }
            
            if (winsTb == null) return;
            sessionWins.TryGetValue(playerIndex, out var wins);
            winsTb.Dispatcher.Invoke(() => winsTb.Text = $"Wins: {wins}");
        }
    }
}