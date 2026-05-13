using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Ardalis.ListStartupServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NimblePros.Metronome;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Seeding Database...");

        using var scope = app.Services.CreateScope();
        try
        {
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider, app.Logger);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }

    public static void UseCustomHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = new
                    {
                        status = report.Status.ToString(),
                        errors = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            value = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                        })
                    }.ToJson();
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });
    }

    public static void UseTroubleshootingMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsDocker())
        {
            app.Logger.LogInformation("Adding Development middleware...");
            app.UseDeveloperExceptionPage();
            app.UseShowAllServicesMiddleware();
            app.UseMigrationsEndPoint();
            app.UseWebAssemblyDebugging();
            app.UseMetronomeLoggingMiddleware();
        }
        else
        {
            app.Logger.LogInformation("Adding non-Development middleware...");
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
    }
}
