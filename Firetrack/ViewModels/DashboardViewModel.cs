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
        public ICommand GoToInventoryCommand { get; }
        public ICommand GoToRequestEquipmentCommand { get; }
        public ICommand ReturnEquipmentCommand { get; }
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
            GoToInventoryCommand = new Command(async () => await Shell.Current.GoToAsync("InventoryPage"));
            GoToRequestEquipmentCommand = new Command(async () => await Shell.Current.GoToAsync("RequestEquipmentPage"));
            ReturnEquipmentCommand = new Command<EquipmentModel>(OnReturnEquipment);
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

        private async void OnReturnEquipment(EquipmentModel? equipment)
        {
            if (equipment == null) return;

            if (App.CurrentUser == null || equipment.AssignedToUsername != App.CurrentUser.Username)
            {
                await Shell.Current.DisplayAlert("Error", "This equipment is not assigned to you.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Return",
                $"Return '{equipment.Name}'?",
                "Yes",
                "Cancel");

            if (!confirm) return;

            try
            {
                equipment.Status = "Available";
                equipment.AssignedToUsername = null;
                equipment.LastUpdated = DateTime.Now;

                var transaction = new TransactionModel
                {
                    EquipmentQR = equipment.QRCode,
                    FromUser = App.CurrentUser.Username,
                    ToUser = "System",
                    Timestamp = DateTime.Now,
                    Action = "Return",
                    Remarks = $"Returned by {App.CurrentUser.FullName}"
                };

                await App.Database!.SaveEquipmentAsync(equipment);
                await App.Database!.SaveTransactionAsync(transaction);

                // ========== SEND NOTIFICATION TO ADMIN ==========
                await App.Database!.SendNotificationAsync(
                    "admin",
                    "↩️ Equipment Returned",
                    $"{App.CurrentUser?.FullName} returned '{equipment.Name}'."
                );
                // ================================================

                await Shell.Current.DisplayAlert("Success", $"'{equipment.Name}' returned successfully.", "OK");
                LoadEquipment(); // refresh the list
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
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