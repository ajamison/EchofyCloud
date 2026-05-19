using Echofy.Application.Interfaces;
using Echofy.Application.Services;
using Echofy.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Echofy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IDealService, DealService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IThankYouNoteService, ThankYouNoteService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IRewardProgramService, RewardProgramService>();

        services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

        return services;
    }
}
