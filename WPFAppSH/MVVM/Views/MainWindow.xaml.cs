using System.Windows;
using WPFAppSH.MVVM.ViewModels;

namespace WPFAppSH.MVVM.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TopWindowBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ResizeMode = ResizeMode.CanResize;
        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}