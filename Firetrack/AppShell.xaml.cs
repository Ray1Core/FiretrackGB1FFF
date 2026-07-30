using Firetrack.Views;

namespace Firetrack;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
#pragma warning disable CA1416

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
        Routing.RegisterRoute("TransferPage", typeof(TransferPage));
        Routing.RegisterRoute("ReportDamagePage", typeof(ReportDamagePage));
        Routing.RegisterRoute("IcsPage", typeof(IcsPage));
        Routing.RegisterRoute("ClearancePage", typeof(ClearancePage));
        Routing.RegisterRoute("AddUserPage", typeof(AddUserPage));
        Routing.RegisterRoute("GenerateQRPage", typeof(GenerateQRPage));
        Routing.RegisterRoute("ScannerPage", typeof(ScannerPage));
        Routing.RegisterRoute("InventoryPage", typeof(InventoryPage));
        Routing.RegisterRoute("RequestEquipmentPage", typeof(RequestEquipmentPage));
        Routing.RegisterRoute("TransactionHistoryPage", typeof(TransactionHistoryPage));
        Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
    }
}