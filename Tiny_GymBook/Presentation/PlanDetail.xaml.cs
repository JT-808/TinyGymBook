using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

namespace Tiny_GymBook.Presentation;

public sealed partial class PlanDetail : Page
{

    public PlanDetail()
    {
        this.InitializeComponent();
        this.DataContextChanged += (s, e) =>
    {
        Debug.WriteLine($"[DEBUG] DataContextChanged! Neuer Typ: {this.DataContext?.GetType().Name}");
    };
    }
}
