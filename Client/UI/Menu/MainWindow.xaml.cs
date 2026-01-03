using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Client.UI.Menu;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.Maximized;
        SwitchContent(new MainMenu());
    }

    [SuppressMessage("Performance", "CA1822")]
    public void SwitchContent(UIElement content)
    {
        MainContent.Content = content;
    }
}
