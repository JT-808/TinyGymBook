
using System.Diagnostics;

namespace Tiny_GymBook.Presentation;


public sealed partial class MainPage : Page
{

    public MainPage()
    {
        this.InitializeComponent();
        Debug.WriteLine($"[DEBUG] MainPage DataContext (CTOR): {this.DataContext}");
        this.Loaded += (s, e) => Debug.WriteLine($"[DEBUG] MainPage DataContext (Loaded): {this.DataContext}");
        this.Loaded += MainPage_Loaded;
    }
    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            await vm.InitAsync(); // Daten werden immer neu geladen!
    }

}
