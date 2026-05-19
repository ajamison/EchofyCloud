using Echofy.Application.Interfaces;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Echofy.Infrastructure.Identity;
using Echofy.Infrastructure.Repositories;
using Echofy.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echofy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient<IRecommendationService, RecommendationHttpService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["RecommendationApi:BaseUrl"] ?? "http://localhost:5100");
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IDealRepository, DealRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteProductRepository, FavoriteProductRepository>();
        services.AddScoped<IReferralRepository, ReferralRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IThankYouNoteRepository, ThankYouNoteRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IRewardProgramRepository, RewardProgramRepository>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IUserLookupService, UserLookupService>();

        return services;
    }
}
