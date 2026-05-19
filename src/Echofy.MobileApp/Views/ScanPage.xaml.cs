using Echofy.MobileApp.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace Echofy.MobileApp.Views;

public partial class ScanPage : ContentPage
{
    private readonly ScanViewModel _viewModel;

    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission Required",
                "Camera permission is needed to scan barcodes.", "OK");
            return;
        }

        BarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats    = BarcodeFormat.Ean13 | BarcodeFormat.Ean8 |
                         BarcodeFormat.UpcA  | BarcodeFormat.UpcE |
                         BarcodeFormat.Code128,
            AutoRotate = true,
            Multiple   = false
        };
        BarcodeReaderView.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BarcodeReaderView.IsDetecting = false;
    }

    private async void BarcodeReaderView_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e) =>
        await _viewModel.OnBarcodeDetectedAsync(e);
}
