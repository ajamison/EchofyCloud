using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Echofy.MobileApp.Models;

namespace Echofy.MobileApp.ViewModels;

[QueryProperty(nameof(Product), "Product")]
public partial class ProductDetailViewModel : ObservableObject
{
    private const string ApiBaseUrl = "https://10.0.2.2:7001";

    [ObservableProperty] private ProductDto? _product;
    [ObservableProperty] private string      _imageSource = string.Empty;

    partial void OnProductChanged(ProductDto? value)
    {
        if (value is null) return;

        var mainImage = value.Images.FirstOrDefault(i => i.IsMain)
                     ?? value.Images.FirstOrDefault();

        ImageSource = mainImage is not null
            ? $"{ApiBaseUrl}{mainImage.Url}"
            : value.ImageUrl ?? string.Empty;
    }

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
