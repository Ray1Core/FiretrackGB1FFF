using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class GenerateQRViewModel : ViewModelBase
    {
        public ICommand GoBackCommand { get; }

        public GenerateQRViewModel()
        {
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }
    }
}