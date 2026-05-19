using CommunityToolkit.Mvvm.ComponentModel;
using Echofy.MobileApp.Services;
using Echofy.MobileApp.Views;
using ZXing.Net.Maui;

namespace Echofy.MobileApp.ViewModels;

public partial class ScanViewModel(IProductService productService) : ObservableObject
{
    [ObservableProperty] private bool   _isBusy        = false;
    [ObservableProperty] private string _statusMessage = "Aim camera at a barcode";
    private bool _isNavigating = false;

    public async Task OnBarcodeDetectedAsync(BarcodeDetectionEventArgs e)
    {
        if (IsBusy || _isNavigating) return;

        var result = e.Results.FirstOrDefault();
        if (result is null) return;

        _isNavigating = true;
        IsBusy = true;
        StatusMessage = "Looking up product...";

        try
        {
            var upc     = result.Value;
            var product = await productService.GetByUpcAsync(upc);

            if (product is null)
            {
                StatusMessage = $"No product found for: {upc}";
                if (Shell.Current.CurrentPage is Page page)
                    await page.DisplayAlertAsync("Not Found", $"No product matches UPC: {upc}", "OK");
                return;
            }

            var navParam = new Dictionary<string, object> { ["Product"] = product };
            await Shell.Current.GoToAsync(nameof(ProductDetailPage), navParam);
        }
        catch (UnauthorizedAccessException)
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            if (Shell.Current.CurrentPage is Page page)
                await page.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            _isNavigating = false;
            StatusMessage = "Aim camera at a barcode";
        }
    }
}
