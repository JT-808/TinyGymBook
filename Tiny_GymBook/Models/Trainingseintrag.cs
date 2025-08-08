using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;
using SQLite;
using Windows.Foundation.Metadata;

namespace Tiny_GymBook.Models;

[Bindable]
[Obsolete("This method is obsolete. Call CallNewMethod instead.", false)]
public class Trainingseintrag
{

    [PrimaryKey, AutoIncrement]
    public int Eintrag_Id { get; set; } // Foreign Key zu Trainingsplan

    [Indexed]
    public int Uebung_Id { get; set; } // Foreign Key zu Uebung

    [Indexed]
    public int Trainingsplan_Id { get; set; }

    public string Kommentar { get; set; } = string.Empty;

    [Indexed]
    public int TagId { get; set; }


    public string Training_Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    [Ignore]
    public Uebung? Uebung { get; set; }

    [Ignore]
    public ObservableCollection<Satz> Saetze { get; set; } = new();



    public Trainingseintrag()
    {
        Saetze = new ObservableCollection<Satz>();
        Kommentar = string.Empty;
        Training_Date = DateTime.Today.ToString("yyyy-MM-dd");
        // Initialisiere Uebung mit leeren/default-Werten, damit das Binding funktioniert!
        Uebung = new Uebung { Name = "", Muskelgruppe = Muskelgruppe.Brust }; // Setze einen Default
    }


}
