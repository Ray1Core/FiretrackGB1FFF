using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class AddEquipmentViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private string _qrCode = string.Empty;
        private string _name = string.Empty;
        private string _type = string.Empty;
        private string _status = "Available";
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public ObservableCollection<string> Types { get; } = new() { "Hose", "Nozzle", "Rescue Tool" };
        public ObservableCollection<string> Statuses { get; } = new() { "Available", "Issued", "Damaged", "InRepair" };

        public string QRCode
        {
            get => _qrCode;
            set { _qrCode = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEquipmentViewModel()
        {
            _db = App.Database!;
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void OnSave()
        {
            if (string.IsNullOrWhiteSpace(QRCode) || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Type))
            {
                StatusMessage = "QR Code, Name, and Type are required.";
                return;
            }

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                // Check if QR code already exists
                var existing = await _db.GetEquipmentByQRAsync(QRCode.Trim());
                if (existing != null)
                {
                    StatusMessage = "❌ QR Code already exists.";
                    IsBusy = false;
                    return;
                }

                var newEquipment = new EquipmentModel
                {
                    QRCode = QRCode.Trim(),
                    Name = Name.Trim(),
                    Type = Type.Trim(),
                    Status = Status,
                    AssignedToUsername = null,
                    LastUpdated = DateTime.Now
                };

                await _db.SaveEquipmentAsync(newEquipment);

                StatusMessage = $"✅ Equipment '{newEquipment.Name}' added successfully!";

                // Clear fields for next entry
                QRCode = string.Empty;
                Name = string.Empty;
                Type = string.Empty;
                Status = "Available";

                // Optionally navigate back after a delay
                await Task.Delay(1500);
                await Shell.Current.GoToAsync("..");
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