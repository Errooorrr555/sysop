using SysOpCore.Models;
using SysOpCore.ViewModels;
using System.Windows;

namespace SysOpCore;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        WinReBanner.Visibility = _vm.IsWinRe ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void RunCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ActionCard card })
        {
            await _vm.ExecuteCardAsync(card);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPane.Visibility = SettingsPane.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AlwaysOnTop_Changed(object sender, RoutedEventArgs e)
    {
        Topmost = _vm.AlwaysOnTop;
    }

    private void BrowseSavePath_Click(object sender, RoutedEventArgs e)
    {
        _vm.Output = "Browse dialog for WinRE path is environment-specific and should be wired to a native folder picker.";
    }

    private void DisableLaunch_Click(object sender, RoutedEventArgs e)
    {
        _vm.Output = "Convenient Launch cleanup stub: remove Win+X/context-menu integration keys here.";
    }
}
