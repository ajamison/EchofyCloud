using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Echofy.MobileApp.Models;
using Echofy.MobileApp.Services;
using Echofy.MobileApp.Views;

namespace Echofy.MobileApp.ViewModels;

public partial class LoginViewModel(IAuthService authService) : ObservableObject
{
    [ObservableProperty] private string _email        = string.Empty;
    [ObservableProperty] private string _password     = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    public bool HasError  => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsNotBusy => !IsBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var response = await authService.LoginAsync(new LoginRequest
            {
                Email    = Email,
                Password = Password
            });

            if (response is null)
            {
                ErrorMessage = "Invalid email or password.";
                return;
            }

            await SecureStorage.SetAsync("jwt_token",  response.Token);
            await SecureStorage.SetAsync("user_email", response.Email);
            await SecureStorage.SetAsync("user_name",  response.FullName);

            await Shell.Current.GoToAsync($"//{nameof(ScanPage)}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
