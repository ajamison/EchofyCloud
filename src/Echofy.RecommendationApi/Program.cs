using Echofy.RecommendationApi.Data;
using Echofy.RecommendationApi.Endpoints;
using Echofy.RecommendationApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RecommendationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<RecommendationEngine>();

var app = builder.Build();

app.MapRecommendationEndpoints();

app.Run();
