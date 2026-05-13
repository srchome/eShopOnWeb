using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider scopedProvider, ILogger logger)
    {
        var catalogContext = scopedProvider.GetRequiredService<CatalogContext>();
        await CatalogContextSeed.SeedAsync(catalogContext, logger);

        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var identityContext = scopedProvider.GetRequiredService<AppIdentityDbContext>();
        await AppIdentityDbContextSeed.SeedAsync(identityContext, userManager, roleManager);

        var openAISettings = scopedProvider.GetRequiredService<IOptions<OpenAISettings>>().Value;
        if (!string.IsNullOrEmpty(openAISettings.ApiKey))
        {
            var embeddingService = scopedProvider.GetRequiredService<IEmbeddingService>();
            await ProductEmbeddingSeeder.SeedAsync(catalogContext, embeddingService, logger);
        }
        else
        {
            logger.LogInformation("OpenAI API key not configured — skipping product embedding seeding.");
        }
    }
}
