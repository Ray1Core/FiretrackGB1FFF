using Firetrack.Models;
using Firetrack.Services;
using Microsoft.Maui.Controls;

namespace Firetrack
{
    public partial class App : Application
    {
        public static UserModel? CurrentUser { get; set; }
        public static DatabaseService? Database { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "firetrack.db3");
            System.Diagnostics.Debug.WriteLine($"📂 Database path: {dbPath}");

            // Delete old database to force fresh seed
            if (File.Exists(dbPath))
            {
                try
                {
                    File.Delete(dbPath);
                    System.Diagnostics.Debug.WriteLine("🗑️ Old database deleted.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Could not delete: {ex.Message}");
                }
            }

            Database = new DatabaseService(dbPath);

            return new Window(new AppShell());
        }
    }
}