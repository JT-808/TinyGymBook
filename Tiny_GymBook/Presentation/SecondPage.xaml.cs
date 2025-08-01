using System.Diagnostics;

namespace Tiny_GymBook.Presentation;

public sealed partial class SecondPage : Page
{
    public SecondPage()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
       {
           Debug.WriteLine($"[DEBUG] Loaded DataContext: {this.DataContext?.GetType().Name}");
       };
    }
}

