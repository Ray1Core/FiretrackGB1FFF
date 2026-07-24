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

        public IcsViewModel(EquipmentModel equipment, UserModel officer)
        {
            _db = App.Database!;
            _pdfService = new PdfGenerationService();
            Equipment = equipment;
            Officer = officer;
            Issuer = App.CurrentUser ?? new UserModel { FullName = "System", Role = "Admin" };

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