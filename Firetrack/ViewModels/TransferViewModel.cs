using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class TransferViewModel : ViewModelBase
    {
        private readonly DatabaseService? _db;
        private UserModel? _selectedOfficer;
        private EquipmentModel? _selectedEquipment;
        private string _manualEquipmentQR = string.Empty;
        private string _statusMessage = string.Empty;

        public ObservableCollection<UserModel> Users { get; set; } = new();
        public ObservableCollection<EquipmentModel> EquipmentList { get; set; } = new();

        public UserModel? SelectedOfficer
        {
            get => _selectedOfficer;
            set { _selectedOfficer = value; OnPropertyChanged(); }
        }

        public EquipmentModel? SelectedEquipment
        {
            get => _selectedEquipment;
            set { _selectedEquipment = value; OnPropertyChanged(); }
        }

        public string ManualEquipmentQR
        {
            get => _manualEquipmentQR;
            set { _manualEquipmentQR = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand TransferCommand { get; }
        public ICommand GoBackCommand { get; }

        public TransferViewModel()
        {
            if (App.CurrentUser?.Role != "Admin")
            {
                StatusMessage = "Access denied. Only Admin can transfer equipment.";
                TransferCommand = new Command(() => { });
                GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
                return;
            }

            _db = App.Database!;
            TransferCommand = new Command(OnTransfer);
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_db == null) return;

            var userList = await _db.GetUsersAsync();
            Users.Clear();
            foreach (var u in userList)
                Users.Add(u);

            var eqList = await _db.GetEquipmentsAsync();
            EquipmentList.Clear();
            foreach (var eq in eqList)
                EquipmentList.Add(eq);
        }

        private async void OnTransfer()
        {
            if (_db == null)
            {
                StatusMessage = "Database not available.";
                return;
            }

            if (SelectedOfficer == null)
            {
                StatusMessage = "Please select the receiving officer.";
                return;
            }

            EquipmentModel? equipment = SelectedEquipment;
            if (equipment == null && !string.IsNullOrWhiteSpace(ManualEquipmentQR))
            {
                equipment = await _db.GetEquipmentByQRAsync(ManualEquipmentQR.Trim());
                if (equipment == null)
                {
                    StatusMessage = "No equipment found with that QR code.";
                    return;
                }
            }
            else if (equipment == null)
            {
                StatusMessage = "Please select or enter an equipment QR code.";
                return;
            }

            var capturedOfficer = SelectedOfficer;
            var capturedEquipment = equipment;

            var transaction = new TransactionModel
            {
                EquipmentQR = capturedEquipment.QRCode,
                FromUser = App.CurrentUser?.Username ?? "admin",
                ToUser = capturedOfficer.Username,
                Timestamp = DateTime.Now,
                Action = "Issue",
                Remarks = $"Issued to {capturedOfficer.FullName}"
            };

            capturedEquipment.AssignedToUsername = capturedOfficer.Username;
            capturedEquipment.Status = "Issued";
            capturedEquipment.LastUpdated = DateTime.Now;

            await _db.SaveTransactionAsync(transaction);
            await _db.SaveEquipmentAsync(capturedEquipment);

            // ========== SEND NOTIFICATION TO RECEIVING OFFICER ==========
            await _db.SendNotificationAsync(
                capturedOfficer.Username,
                "🔄 Equipment Issued",
                $"{App.CurrentUser?.FullName} issued '{capturedEquipment.Name}' to you."
            );
            // =============================================================

            StatusMessage = $"✅ Equipment '{capturedEquipment.Name}' issued to {capturedOfficer.FullName}.";

            SelectedEquipment = null;
            ManualEquipmentQR = string.Empty;
            SelectedOfficer = null;

            await LoadDataAsync();

            var navParams = new Dictionary<string, object>
            {
                { "equipment", capturedEquipment },
                { "officer", capturedOfficer }
            };
            await Shell.Current.GoToAsync("IcsPage", navParams);
        }
    }
}