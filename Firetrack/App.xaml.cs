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
            // Connection string for local SQL Server (Windows Authentication)
            string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=FiretrackDB;Trusted_Connection=True;";

            Database = new DatabaseService(connectionString);

            return new Window(new AppShell());
        }
    }
}