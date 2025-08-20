using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

namespace Tiny_GymBook.Presentation;

public sealed partial class SecondPage : Page
{
    public SecondPage()
    {
        this.InitializeComponent();

    }


    private void OnPlanItemClick(object sender, ItemClickEventArgs e)
    {
        // e.ClickedItem ist der Trainingsplan
        if (DataContext is Tiny_GymBook.Presentation.SecondViewModel vm &&
            vm.OpenPlanCommand?.CanExecute(e.ClickedItem) == true)
        {
            vm.OpenPlanCommand.Execute(e.ClickedItem);
        }
    }
}

