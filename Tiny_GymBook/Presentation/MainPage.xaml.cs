using CommunityToolkit.Mvvm.Input; // für IAsyncRelayCommand<T>
using Microsoft.UI.Xaml;
using Tiny_GymBook.Models;

namespace Tiny_GymBook.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage() => InitializeComponent();

    private async void OnSatzFieldLostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm &&
            sender is FrameworkElement fe &&
            fe.DataContext is Satz satz &&
            vm.SaveSatzLeanCommand is IAsyncRelayCommand<Satz> cmd)
        {
            if (cmd.CanExecute(satz))
                await cmd.ExecuteAsync(satz);
        }
    }
}
