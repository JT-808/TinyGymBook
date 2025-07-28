
namespace Tiny_GymBook.Presentation;


public sealed partial class MainPage : Page
{

    public MainPage()
    {
        this.InitializeComponent();
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
