using ATGSaveGameManager.ViewModel;
using Avalonia.Controls;

namespace ATGSaveGameManager.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


    }

    public void Initizalize()
    {
        var context = (MainViewModel)DataContext;
        Loaded += async (_, __) => await context.InitializeAsync();
    }
}