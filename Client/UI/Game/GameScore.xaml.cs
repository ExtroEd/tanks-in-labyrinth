using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
}
