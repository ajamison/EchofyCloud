using Echofy.MobileApp.ViewModels;

namespace Echofy.MobileApp.Views;

public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
