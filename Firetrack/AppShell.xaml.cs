using Firetrack.Views;

namespace Firetrack;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
#pragma warning disable CA1416

        // Routes are already defined in XAML, but we keep them here too.
        // This ensures compatibility with our GoToAsync calls.
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
        Routing.RegisterRoute("InventoryPage", typeof(InventoryPage));
        Routing.RegisterRoute("TransferPage", typeof(TransferPage));
        Routing.RegisterRoute("AddUserPage", typeof(AddUserPage));
        Routing.RegisterRoute("ClearancePage", typeof(ClearancePage));
        Routing.RegisterRoute("AddEquipmentPage", typeof(AddEquipmentPage));
        Routing.RegisterRoute("RequestEquipmentPage", typeof(RequestEquipmentPage));
        Routing.RegisterRoute("ReportDamagePage", typeof(ReportDamagePage));
        Routing.RegisterRoute("IcsPage", typeof(IcsPage));
        Routing.RegisterRoute("TransactionHistoryPage", typeof(TransactionHistoryPage));
        Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
        Routing.RegisterRoute("GenerateQRPage", typeof(GenerateQRPage));
        Routing.RegisterRoute("ScannerPage", typeof(ScannerPage));
        Routing.RegisterRoute("UserManagementPage", typeof(UserManagementPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
        Routing.RegisterRoute("PendingRequestsPage", typeof(PendingRequestsPage));   // NEW
    }
}