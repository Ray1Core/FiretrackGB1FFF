using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private string _welcomeMessage = string.Empty;
        private ObservableCollection<EquipmentModel> _myEquipment = new();
        private bool _isAdmin;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EquipmentModel> MyEquipment
        {
            get => _myEquipment;
            set { _myEquipment = value; OnPropertyChanged(); }
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set { _isAdmin = value; OnPropertyChanged(); }
        }

        public string UserRole => App.CurrentUser?.Role ?? "Guest";

        public ICommand GoToScannerCommand { get; }
        public ICommand GoToGenerateCommand { get; }
        public ICommand GoToTransferCommand { get; }
        public ICommand GoToAddUserCommand { get; }
        public ICommand GoToClearanceCommand { get; }
        public ICommand ReportDamageCommand { get; }
        public ICommand LogoutCommand { get; }

        public DashboardViewModel()
        {
            var user = App.CurrentUser;
            WelcomeMessage = $"Welcome, {user?.FullName ?? "Firefighter"}!";
            IsAdmin = user?.Role == "Admin";

            LogoutCommand = new Command(OnLogout);
            GoToScannerCommand = new Command(async () => await Shell.Current.GoToAsync("ScannerPage"));
            GoToGenerateCommand = new Command(async () => await Shell.Current.GoToAsync("GenerateQRPage"));
            GoToTransferCommand = new Command(async () => await Shell.Current.GoToAsync("TransferPage"));
            GoToAddUserCommand = new Command(async () => await Shell.Current.GoToAsync("AddUserPage"));
            GoToClearanceCommand = new Command(async () => await Shell.Current.GoToAsync("ClearancePage"));
            ReportDamageCommand = new Command<EquipmentModel>(OnReportDamage);

            LoadEquipment();
        }

        private async void LoadEquipment()
        {
            if (App.CurrentUser == null) return;
            var db = App.Database;
            if (db == null) return;

            var equipment = await db.GetEquipmentsAssignedToUserAsync(App.CurrentUser.Username);
            MyEquipment.Clear();
            foreach (var item in equipment)
                MyEquipment.Add(item);
        }

        private async void OnReportDamage(EquipmentModel equipment)
        {
            if (equipment == null) return;

            var navigationParams = new Dictionary<string, object>
            {
                { "equipment", equipment }
            };
            await Shell.Current.GoToAsync("ReportDamagePage", navigationParams);
        }

        private async void OnLogout()
        {
            App.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}