using Echofy.MobileApp.Services;
using Echofy.MobileApp.ViewModels;
using Echofy.MobileApp.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace Echofy.MobileApp;

public static class MauiProgram
{
    // Update this to your machine's IP when testing on a physical device.
    // Use https://10.0.2.2:7001/ for the Android emulator (maps to host localhost).
    private const string ApiBaseUrl = "https://192.168.1.245:7001/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // Trust dev certificate on Android emulator
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(15);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        builder.Services.AddHttpClient<IProductService, ProductService>(client =>
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(15);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
#else
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout     = TimeSpan.FromSeconds(15);
        });

        builder.Services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout     = TimeSpan.FromSeconds(15);
        });
#endif

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<ProductDetailPage>();


        return builder.Build();
    }
}
