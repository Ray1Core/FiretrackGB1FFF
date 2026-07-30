using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class RequestEquipmentViewModel : ViewModelBase
    {
        private ObservableCollection<EquipmentModel> _availableEquipment = new();
        private EquipmentModel? _selectedEquipment;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public ObservableCollection<EquipmentModel> AvailableEquipment
        {
            get => _availableEquipment;
            set { _availableEquipment = value; OnPropertyChanged(); }
        }

        public EquipmentModel? SelectedEquipment
        {
            get => _selectedEquipment;
            set { _selectedEquipment = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoadAvailableCommand { get; }
        public ICommand RequestCommand { get; }
        public ICommand GoBackCommand { get; }

        public RequestEquipmentViewModel()
        {
            LoadAvailableCommand = new Command(async () => await OnLoadAvailable());   // <-- changed to async
            RequestCommand = new Command(OnRequest);
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            _ = OnLoadAvailable();   // fire-and-forget on startup
        }

        // ===== CHANGED: async void → async Task =====
        private async Task OnLoadAvailable()
        {
            if (App.Database == null) return;

            IsBusy = true;
            try
            {
                var all = await App.Database.GetEquipmentsAsync();
                var available = all.Where(e => e.Status == "Available" && string.IsNullOrEmpty(e.AssignedToUsername));
                AvailableEquipment.Clear();
                foreach (var item in available)
                    AvailableEquipment.Add(item);
                StatusMessage = $"📋 {AvailableEquipment.Count} available equipment(s).";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnRequest()
        {
            if (SelectedEquipment == null)
            {
                StatusMessage = "Please select an equipment first.";
                return;
            }

            if (App.CurrentUser == null)
            {
                StatusMessage = "You must be logged in.";
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Confirm Request",
                $"Request '{SelectedEquipment.Name}'?",
                "Yes",
                "Cancel");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                var equipment = SelectedEquipment;
                equipment.Status = "Issued";
                equipment.AssignedToUsername = App.CurrentUser.Username;
                equipment.LastUpdated = DateTime.Now;

                var transaction = new TransactionModel
                {
                    EquipmentQR = equipment.QRCode,
                    FromUser = "System",
                    ToUser = App.CurrentUser.Username,
                    Timestamp = DateTime.Now,
                    Action = "Issue",
                    Remarks = $"Requested by {App.CurrentUser.FullName}"
                };

                await App.Database!.SaveEquipmentAsync(equipment);
                await App.Database!.SaveTransactionAsync(transaction);

                // ========== SEND NOTIFICATION TO ADMIN ==========
                await App.Database!.SendNotificationAsync(
                    "admin",
                    "📋 Equipment Request",
                    $"{App.CurrentUser?.FullName} requested '{equipment.Name}'."
                );
                // ================================================

                StatusMessage = $"✅ {equipment.Name} assigned to you!";
                SelectedEquipment = null;
                await OnLoadAvailable(); // now await works ✅
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}