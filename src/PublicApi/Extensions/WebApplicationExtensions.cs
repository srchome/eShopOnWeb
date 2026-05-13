using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

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
}
