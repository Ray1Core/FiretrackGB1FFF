using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Firetrack.Models;
using Microsoft.Maui.Controls;

namespace Firetrack.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLogin);
        }

        private async void OnLogin()
        {
            try
            {
                var db = App.Database;
                if (db == null)
                {
                    ErrorMessage = "Database not available.";
                    return;
                }

                IsBusy = true;
                ErrorMessage = string.Empty;

                // --- DEBUG: print all users ---
                var allUsers = await db.GetUsersAsync();
                System.Diagnostics.Debug.WriteLine($"📋 All users in DB ({allUsers.Count}):");
                foreach (var u in allUsers)
                {
                    System.Diagnostics.Debug.WriteLine($"   👤 {u.Username} : {u.Password}");
                }
                // --- end debug ---

                var user = await db.GetUserByUsernameAsync(Username);
                if (user != null && user.Password == Password)
                {
                    App.CurrentUser = user;
                    await Shell.Current.GoToAsync("//DashboardPage");
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    System.Diagnostics.Debug.WriteLine($"❌ Login failed for '{Username}'");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"🚨 Login error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}