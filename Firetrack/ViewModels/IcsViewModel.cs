using Firetrack.Models;
using Firetrack.Services;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;

namespace Firetrack.ViewModels
{
    public class IcsViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private readonly PdfGenerationService _pdfService;
        private EquipmentModel? _equipment;
        private UserModel? _officer;
        private UserModel? _issuer;
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public EquipmentModel Equipment
        {
            get => _equipment!;
            set { _equipment = value; OnPropertyChanged(); }
        }

        public UserModel Officer
        {
            get => _officer!;
            set { _officer = value; OnPropertyChanged(); }
        }

        public UserModel Issuer
        {
            get => _issuer!;
            set { _issuer = value; OnPropertyChanged(); }
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

        public ICommand GenerateIcsCommand { get; }
        public ICommand GoBackCommand { get; }

        // 👇 Constructor now accepts nullable parameters
        public IcsViewModel(EquipmentModel? equipment, UserModel? officer)
        {
            // DEBUG: Log received parameters
            System.Diagnostics.Debug.WriteLine($"📄 IcsViewModel: Received Equipment = {(equipment?.Name ?? "NULL")}");
            System.Diagnostics.Debug.WriteLine($"📄 IcsViewModel: Received Officer = {(officer?.FullName ?? "NULL")}");

            _db = App.Database!;
            _pdfService = new PdfGenerationService();

            // --- Equipment ---
            if (equipment == null)
            {
                Equipment = new EquipmentModel
                {
                    Name = "Unknown Equipment",
                    QRCode = "N/A",
                    Type = "N/A",
                    Status = "N/A"
                };
                System.Diagnostics.Debug.WriteLine("⚠️ Equipment was null, using fallback.");
            }
            else
            {
                Equipment = equipment;
                // Ensure properties are not empty
                if (string.IsNullOrEmpty(Equipment.Name))
                    Equipment.Name = "Unknown Equipment";
                if (string.IsNullOrEmpty(Equipment.QRCode))
                    Equipment.QRCode = "N/A";
                if (string.IsNullOrEmpty(Equipment.Type))
                    Equipment.Type = "N/A";
                if (string.IsNullOrEmpty(Equipment.Status))
                    Equipment.Status = "N/A";
            }

            // --- Officer ---
            if (officer == null)
            {
                Officer = new UserModel
                {
                    FullName = "Unknown Officer",
                    Username = "N/A",
                    Role = "N/A"
                };
                System.Diagnostics.Debug.WriteLine("⚠️ Officer was null, using fallback.");
            }
            else
            {
                Officer = officer;
                if (string.IsNullOrEmpty(Officer.FullName))
                    Officer.FullName = "Unknown Officer";
                if (string.IsNullOrEmpty(Officer.Username))
                    Officer.Username = "N/A";
                if (string.IsNullOrEmpty(Officer.Role))
                    Officer.Role = "N/A";
            }

            // --- Issuer ---
            Issuer = App.CurrentUser ?? new UserModel
            {
                FullName = "System",
                Role = "Admin",
                Username = "system"
            };
            if (string.IsNullOrEmpty(Issuer.FullName))
                Issuer.FullName = "System";
            if (string.IsNullOrEmpty(Issuer.Role))
                Issuer.Role = "Admin";
            if (string.IsNullOrEmpty(Issuer.Username))
                Issuer.Username = "system";

            System.Diagnostics.Debug.WriteLine($"✅ Equipment set: {Equipment.Name}, QR: {Equipment.QRCode}, Type: {Equipment.Type}");
            System.Diagnostics.Debug.WriteLine($"✅ Officer set: {Officer.FullName}, Username: {Officer.Username}, Role: {Officer.Role}");
            System.Diagnostics.Debug.WriteLine($"✅ Issuer set: {Issuer.FullName}, Role: {Issuer.Role}");

            GenerateIcsCommand = new Command(OnGenerateIcs);
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void OnGenerateIcs()
        {
            IsBusy = true;
            StatusMessage = "Generating PDF...";

            try
            {
                var pdfBytes = _pdfService.GenerateIcsPdf(Equipment, Officer, Issuer);

                var fileName = $"ICS_{Equipment.QRCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var downloadsPath = Path.Combine(FileSystem.AppDataDirectory, "ICS");

                if (!Directory.Exists(downloadsPath))
                    Directory.CreateDirectory(downloadsPath);

                var filePath = Path.Combine(downloadsPath, fileName);
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                StatusMessage = $"✅ ICS saved to: {filePath}";
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
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