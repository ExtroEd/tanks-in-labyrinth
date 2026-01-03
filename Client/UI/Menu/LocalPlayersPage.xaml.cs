using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.UI.Menu;

public partial class LocalPlayersPage
{
   private readonly int _playerCount;

   public LocalPlayersPage(int players)
   {
       InitializeComponent();
       _playerCount = players;

       SetupTanks();
       ApplyPlayerCount();
       ReorderPlayers();

       Focusable = true;
       Loaded += (_, _) => Focus();
       KeyDown += OnPreviewKeyDown;
       KeyUp += OnPreviewKeyUp;
       
       PreviewMouseDown += OnBackgroundClick;

       if (_playerCount < 2) return;
       {
           MouseDown += (_, _) => IndicatorP2.Fill = Brushes.Red;
           MouseUp += (_, _) => IndicatorP2.Fill = Brushes.Transparent;
       }
   }

   private void ReorderPlayers()
   {
       DetachContainers();
       PlayersGrid.Children.Clear();
       PlayersGrid.Columns = _playerCount;

       switch (_playerCount)
       {
           case 1:
               AddContainer(P1Container);
               break;
           case 2:
               AddContainer(P1Container);
               AddContainer(P2Container);
               break;
           case 3:
               AddContainer(P1Container);
               AddContainer(P3Container);
               AddContainer(P2Container);
               break;
           case 4:
               AddContainer(P1Container);
               AddContainer(P4Container);
               AddContainer(P3Container);
               AddContainer(P2Container);
               break;
       }
   }

   private void DetachContainers()
   {
       StackPanel[] containers = [P1Container, P2Container, P3Container, P4Container
       ];
       foreach (var container in containers)
       {
           if (VisualTreeHelper.GetParent(container) is Panel parent)
           {
               parent.Children.Remove(container);
           }
       }
   }

   private void AddContainer(StackPanel container)
   {
       PlayersGrid.Children.Add(container);
   }

   private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
   {
       if (e.OriginalSource is TextBox) return;
       Keyboard.ClearFocus();
       Focus();
   }
   
   private void ApplyPlayerCount()
   {
       P2Container.Visibility = _playerCount >= 2 ? Visibility.Visible : Visibility.Collapsed;
       P3Container.Visibility = _playerCount >= 3 ? Visibility.Visible : Visibility.Collapsed;
       P4Container.Visibility = _playerCount >= 4 ? Visibility.Visible : Visibility.Collapsed;
   }
   
   private void NameInput_KeyDown(object sender, KeyEventArgs e)
   {
       if (e.Key != Key.Enter) return;
       Keyboard.ClearFocus();
       Focus();
   }
   
   private void SetupTanks()
   {
       TankP1.SetColor(Colors.Green, Colors.Green);
       TankP2.SetColor(Colors.Red, Colors.Red);
       TankP3.SetColor(Colors.Blue, Colors.Blue);
       TankP4.SetColor(Colors.Yellow, Colors.Yellow);
   }

   private void OnPreviewKeyDown(object sender, KeyEventArgs e)
   {
       switch (e.Key)
       {
           case Key.OemTilde:
               if (_playerCount >= 1) IndicatorP1.Fill = Brushes.Green; 
               break;
           
           case Key.Add:
               if (_playerCount >= 3) IndicatorP3.Fill = Brushes.Blue; 
               break;
           
           case Key.OemQuotes:
               if (_playerCount >= 4) IndicatorP4.Fill = Brushes.Yellow; 
               break;
       }
   }

   private void OnPreviewKeyUp(object sender, KeyEventArgs e)
   {
       switch (e.Key)
       {
           case Key.OemTilde:
               IndicatorP1.Fill = Brushes.Transparent; 
               break;
           
           case Key.Add:
               IndicatorP3.Fill = Brushes.Transparent; 
               break;
           
           case Key.OemQuotes:
               IndicatorP4.Fill = Brushes.Transparent; 
               break;
       }
   }

   private void StartGame_Click(object sender, RoutedEventArgs e)
   {
       if (Application.Current.MainWindow is not MainWindow main) return;
       
       var playerNames = new string[4];
   
       playerNames[0] = NameP1.Text;
       playerNames[1] = NameP2.Text;
       playerNames[2] = NameP3.Text;
       playerNames[3] = NameP4.Text;

       main.SwitchContent(new Game.Game(_playerCount, playerNames));
   }

   private void Back_Click(object sender, RoutedEventArgs e)
   {
       if (Application.Current.MainWindow is MainWindow main)
       {
           main.SwitchContent(new LocalGamePage());
       }
   }
}
