using Echofy.MobileApp.Views;

namespace Echofy.MobileApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ScanPage is in the Shell visual hierarchy — only ProductDetailPage is a global route
        Routing.RegisterRoute(nameof(ProductDetailPage), typeof(ProductDetailPage));
    }
}
