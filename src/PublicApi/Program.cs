using System;
using System.IO;
using BlazorShared;
using DotNetEnv;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi;
using Microsoft.eShopWeb.PublicApi.Extensions;
using Microsoft.eShopWeb.PublicApi.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NimblePros.Metronome;

var builder = WebApplication.CreateBuilder(args);

// Load .env file for local development
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

// Set authorization secrets before service registration
var jwtSecret = builder.Configuration["AuthorizationConstants:JWT_SECRET_KEY"]
    ?? "test-jwt-secret-key-minimum-32-characters-long";

if (string.IsNullOrEmpty(builder.Configuration["AuthorizationConstants:JWT_SECRET_KEY"]) && !builder.Environment.IsDevelopment() && builder.Environment.EnvironmentName != "Testing")
{
    throw new InvalidOperationException("AuthorizationConstants:JWT_SECRET_KEY not configured. Check your .env file.");
}

AuthorizationConstants.JWT_SECRET_KEY = jwtSecret;

// Add service defaults & Aspire components.
builder.AddAspireServiceDefaults();

builder.Services.AddFastEndpoints();

// Use to force loading of appsettings.json of test project
builder.Configuration.AddConfigurationFile("appsettings.test.json");

builder.Services.ConfigureLocalDatabaseContexts(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppIdentityDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddCustomServices(builder.Configuration);

builder.Services.AddMemoryCache();

builder.Services.AddJwtAuthentication();

const string CORS_POLICY = "CorsPolicy";

var configSection = builder.Configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
builder.Services.Configure<BaseUrlConfiguration>(configSection);
var baseUrlConfig = configSection.Get<BaseUrlConfiguration>();
builder.Services.AddCorsPolicy(CORS_POLICY, baseUrlConfig!);

builder.Services.AddControllers();

// TODO: Consider removing AutoMapper dependency (FastEndpoints already has its own Mapper support)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddSwagger();

builder.Services.AddMetronome();
string seqUrl = builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341";

builder.AddSeqEndpoint(connectionName: "seq", options =>
{
    options.ServerUrl = seqUrl;
});

var app = builder.Build();

app.Logger.LogInformation("PublicApi App created...");

await app.SeedDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(CORS_POLICY);

app.UseAuthorization();

app.UseFastEndpoints();

app.UseSwaggerGen();

app.Logger.LogInformation("LAUNCHING PublicApi");
app.Run();

public partial class Program { }
