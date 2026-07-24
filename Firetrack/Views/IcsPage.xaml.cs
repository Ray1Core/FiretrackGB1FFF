using Firetrack.Models;
using Firetrack.ViewModels;
using Microsoft.Maui.Controls;

namespace Firetrack.Views;

public partial class IcsPage : ContentPage, IQueryAttributable
{
    public IcsPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("equipment", out var eqObj) && eqObj is EquipmentModel equipment &&
            query.TryGetValue("officer", out var offObj) && offObj is UserModel officer)
        {
            BindingContext = new IcsViewModel(equipment, officer);
        }
    }
}