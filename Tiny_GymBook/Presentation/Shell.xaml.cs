using System.Diagnostics;

namespace Tiny_GymBook.Presentation;

public sealed partial class Shell : UserControl
{
    public Shell()
    {
        this.InitializeComponent();
        Debug.WriteLine($"[DEBUG] Shell DataContext: {this.DataContext}");
    }
}
