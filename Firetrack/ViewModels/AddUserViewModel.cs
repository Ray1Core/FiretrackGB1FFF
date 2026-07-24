using Firetrack.Models;
using Firetrack.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class AddUserViewModel : ViewModelBase
    {
        private readonly DatabaseService _db;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _fullName = string.Empty;
        private string _role = "Personnel";
        private string _statusMessage = string.Empty;
        private bool _isBusy;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles { get; } = new() { "Admin", "Personnel" };

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

        public ICommand SaveUserCommand { get; }
        public ICommand CancelCommand { get; }

        public AddUserViewModel()
        {
            _db = App.Database!;
            SaveUserCommand = new Command(OnSaveUser);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void OnSaveUser()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "All fields are required.";
                return;
            }

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var existing = await _db.GetUserByUsernameAsync(Username);
                if (existing != null)
                {
                    StatusMessage = "❌ Username already exists.";
                    IsBusy = false;
                    return;
                }

                var newUser = new UserModel
                {
                    Username = Username.Trim(),
                    Password = Password.Trim(),
                    FullName = FullName.Trim(),
                    Role = Role
                };

                await _db.SaveUserAsync(newUser);

                StatusMessage = $"✅ User '{newUser.Username}' created successfully!";
                Username = string.Empty;
                Password = string.Empty;
                FullName = string.Empty;
                Role = "Personnel";
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