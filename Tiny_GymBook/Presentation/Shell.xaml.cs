using System.Diagnostics;

namespace Tiny_GymBook.Presentation;

public sealed partial class Shell : UserControl
{
    public Shell()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Loaded DataContext (Shell): {this.DataContext?.GetType().Name}");
        };
    }
}
