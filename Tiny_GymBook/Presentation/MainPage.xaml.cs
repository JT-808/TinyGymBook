
using System.Diagnostics;

namespace Tiny_GymBook.Presentation;


public sealed partial class MainPage : Page
{

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += async (s, e) =>
       {
           await Task.Delay(500);
           Debug.WriteLine($"[DEBUG] Loaded DataContext (MainPage, 500ms später): {this.DataContext?.GetType().Name}");
       };

    }


    private void AddSet_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Trainingseintrag eintrag
            && DataContext is MainViewModel vm)
        {
            vm.AddSatz(eintrag);
        }
    }
}
