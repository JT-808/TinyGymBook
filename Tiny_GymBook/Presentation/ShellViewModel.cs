using System.Diagnostics;
using Microsoft.UI.Xaml.Data;

namespace Tiny_GymBook.Presentation;

[Bindable]
public class ShellViewModel
{
    private readonly INavigator _navigator;

    public ShellViewModel(
        INavigator navigator)
    {
        Debug.WriteLine("[DEBUG] ShellViewModel Konstruktor aufgerufen!");
        _navigator = navigator;
        // Add code here to initialize or attach event handlers to singleton services
    }
}
