using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public class CatalogContextSeed
{
    public static async Task SeedAsync(CatalogContext catalogContext,
        ILogger logger,
        int retry = 0)
    {
        var retryForAvailability = retry;
        try
        {
            if (catalogContext.Database.IsSqlServer())
            {
                catalogContext.Database.Migrate();
            }

            if (!await catalogContext.CatalogBrands.AnyAsync())
            {
                await catalogContext.CatalogBrands.AddRangeAsync(
                    GetPreconfiguredCatalogBrands());

                await catalogContext.SaveChangesAsync();
            }

            if (!await catalogContext.CatalogTypes.AnyAsync())
            {
                await catalogContext.CatalogTypes.AddRangeAsync(
                    GetPreconfiguredCatalogTypes());

                await catalogContext.SaveChangesAsync();
            }

            if (!await catalogContext.CatalogItems.AnyAsync())
            {
                await catalogContext.CatalogItems.AddRangeAsync(
                    GetPreconfiguredItems());

                await catalogContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            if (retryForAvailability >= 10) throw;

            retryForAvailability++;
            
            logger.LogError(ex.Message);
            await SeedAsync(catalogContext, logger, retryForAvailability);
            throw;
        }
    }

    static IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
    {
        return new List<CatalogBrand>
            {
                new("Azure"),
                new(".NET"),
                new("Visual Studio"),
                new("SQL Server"),
                new("Other")
            };
    }

    static IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
    {
        return new List<CatalogType>
            {
                new("Mug"),
                new("T-Shirt"),
                new("Sheet"),
                new("USB Memory Stick")
            };
    }

    static IEnumerable<CatalogItem> GetPreconfiguredItems()
    {
        const string baseUrl = UriComposer.CatalogBaseUrlPlaceholder;
        return new List<CatalogItem>
            {
                // Original 12 items
                new(2,2, ".NET Bot Black Sweatshirt", ".NET Bot Black Sweatshirt", 19.5M,  $"{baseUrl}/images/products/1.png"),
                new(1,2, ".NET Black & White Mug", ".NET Black & White Mug", 8.50M, $"{baseUrl}/images/products/2.png"),
                new(2,5, "Prism White T-Shirt", "Prism White T-Shirt", 12,  $"{baseUrl}/images/products/3.png"),
                new(2,2, ".NET Foundation Sweatshirt", ".NET Foundation Sweatshirt", 12, $"{baseUrl}/images/products/4.png"),
                new(3,5, "Roslyn Red Sheet", "Roslyn Red Sheet", 8.5M, $"{baseUrl}/images/products/5.png"),
                new(2,2, ".NET Blue Sweatshirt", ".NET Blue Sweatshirt", 12, $"{baseUrl}/images/products/6.png"),
                new(2,5, "Roslyn Red T-Shirt", "Roslyn Red T-Shirt",  12, $"{baseUrl}/images/products/7.png"),
                new(2,5, "Kudu Purple Sweatshirt", "Kudu Purple Sweatshirt", 8.5M, $"{baseUrl}/images/products/8.png"),
                new(1,5, "Cup<T> White Mug", "Cup<T> White Mug", 12, $"{baseUrl}/images/products/9.png"),
                new(3,2, ".NET Foundation Sheet", ".NET Foundation Sheet", 12, $"{baseUrl}/images/products/10.png"),
                new(3,2, "Cup<T> Sheet", "Cup<T> Sheet", 8.5M, $"{baseUrl}/images/products/11.png"),
                new(2,5, "Prism White TShirt", "Prism White TShirt", 12, $"{baseUrl}/images/products/12.png"),

                // T-Shirts & Sweatshirts (type 2)
                new(2,1, "Lightweight Azure-branded t-shirt for cloud developers. Features the Azure logo on a sky-blue background.", "Azure Cloud Developer T-Shirt", 15M, $"{baseUrl}/images/products/1.png"),
                new(2,1, "Navy t-shirt celebrating the Azure DevOps CI/CD platform. Perfect for pipeline engineers.", "Azure DevOps Navy T-Shirt", 15M, $"{baseUrl}/images/products/2.png"),
                new(2,1, "Warm hoodie with the Azure Functions serverless logo. Great for late-night coding sessions.", "Azure Functions Hoodie", 24M, $"{baseUrl}/images/products/3.png"),
                new(2,1, "Cosy sweatshirt featuring the Azure cloud logo. Stay warm while building cloud solutions.", "Azure Cloud Sweatshirt", 22M, $"{baseUrl}/images/products/4.png"),
                new(2,3, "Black t-shirt inspired by the popular Visual Studio dark theme. For developers who prefer dark mode.", "Visual Studio Dark Theme T-Shirt", 15M, $"{baseUrl}/images/products/5.png"),
                new(2,3, "Celebrate the world's most popular code editor with this VS Code branded t-shirt.", "Visual Studio Code T-Shirt", 15M, $"{baseUrl}/images/products/6.png"),
                new(2,3, "Comfortable purple sweatshirt featuring the Visual Studio logo. Ideal for long coding days.", "Visual Studio Purple Sweatshirt", 22M, $"{baseUrl}/images/products/7.png"),
                new(2,4, "White t-shirt for SQL Server database professionals. Features a clever SQL query graphic.", "SQL Server Query T-Shirt", 15M, $"{baseUrl}/images/products/8.png"),
                new(2,4, "Warm sweatshirt for database administrators. Proudly display your SQL Server expertise.", "SQL Server DBA Sweatshirt", 22M, $"{baseUrl}/images/products/9.png"),
                new(2,2, "Celebrate cross-platform mobile development with this .NET MAUI t-shirt. Lightweight and comfortable.", ".NET MAUI Mobile T-Shirt", 15M, $"{baseUrl}/images/products/10.png"),
                new(2,2, "Purple t-shirt for Blazor WebAssembly enthusiasts. Build beautiful web UIs with C#!", ".NET Blazor Purple T-Shirt", 15M, $"{baseUrl}/images/products/11.png"),
                new(2,2, "Classic .NET Core t-shirt for backend developers. Soft cotton, great for everyday wear.", ".NET Core Developer T-Shirt", 15M, $"{baseUrl}/images/products/12.png"),
                new(2,2, "Celebrate the latest LTS release with this bold red .NET 8 t-shirt.", ".NET 8 Red T-Shirt", 15M, $"{baseUrl}/images/products/1.png"),
                new(2,2, "Celebrate the elegance of .NET Minimal APIs with this clean, minimalist t-shirt design.", ".NET Minimal API T-Shirt", 15M, $"{baseUrl}/images/products/2.png"),
                new(2,5, "Container your passion! Blue t-shirt with the Docker whale logo for DevOps engineers.", "Docker Blue T-Shirt", 15M, $"{baseUrl}/images/products/3.png"),
                new(2,5, "Orchestrate your wardrobe with this Kubernetes-branded t-shirt. For cloud-native enthusiasts.", "Kubernetes T-Shirt", 15M, $"{baseUrl}/images/products/4.png"),
                new(2,5, "Dark mode for your wardrobe. This GitHub Octocat t-shirt is a developer essential.", "GitHub Dark T-Shirt", 15M, $"{baseUrl}/images/products/5.png"),
                new(2,5, "Show your love for open source software with this community-inspired t-shirt.", "Open Source T-Shirt", 15M, $"{baseUrl}/images/products/6.png"),
                new(2,5, "Cosy sweatshirt for DevOps engineers. Features CI/CD pipeline artwork on a grey background.", "DevOps Pipeline Sweatshirt", 22M, $"{baseUrl}/images/products/7.png"),
                new(2,2, "Warm sweatshirt celebrating the vibrant .NET developer community worldwide.", ".NET Community Sweatshirt", 22M, $"{baseUrl}/images/products/8.png"),

                // Mugs (type 1)
                new(1,1, "White ceramic mug featuring the Azure portal dashboard design. 350ml capacity for your favourite beverage.", "Azure Portal Mug", 9M, $"{baseUrl}/images/products/9.png"),
                new(1,1, "Sky-blue mug with the Azure cloud logo. Start your cloud journey with your morning coffee.", "Azure Cloud Blue Mug", 9M, $"{baseUrl}/images/products/10.png"),
                new(1,3, "Mug for developers who live in the debugger. Features a breakpoint graphic and the VS logo.", "Visual Studio Debug Mug", 9M, $"{baseUrl}/images/products/11.png"),
                new(1,3, "Celebrate the magic of IntelliSense with this Visual Studio branded mug.", "Visual Studio IntelliSense Mug", 9M, $"{baseUrl}/images/products/12.png"),
                new(1,4, "Black mug for SQL Server professionals. Perfect for those long query-optimisation sessions.", "SQL Server Data Mug", 9M, $"{baseUrl}/images/products/1.png"),
                new(1,4, "Sleek black ceramic mug with the SQL Server logo. For database administrators who mean business.", "SQL Server Black Mug", 9M, $"{baseUrl}/images/products/2.png"),
                new(1,2, "Classic white mug with the .NET Core logo. An essential item for any .NET developer.", ".NET Core White Mug", 9M, $"{baseUrl}/images/products/3.png"),
                new(1,2, "Blue mug celebrating the .NET Foundation and its mission to support the .NET ecosystem.", ".NET Foundation Blue Mug", 9M, $"{baseUrl}/images/products/4.png"),
                new(1,5, "For C# lovers everywhere. This mug features C# syntax art on a clean white background.", "C# Developer Mug", 9M, $"{baseUrl}/images/products/5.png"),
                new(1,5, "The perfect mug for developers who run on coffee. Features a fun coffee-and-code graphic.", "Coffee and Code Mug", 9M, $"{baseUrl}/images/products/6.png"),
                new(1,5, "White mug illustrating a microservices architecture diagram. Great conversation starter.", "Microservices Architecture Mug", 9M, $"{baseUrl}/images/products/7.png"),
                new(1,5, "Mug for the DevOps professional. Celebrate continuous delivery with your morning brew.", "DevOps Engineer Mug", 9M, $"{baseUrl}/images/products/8.png"),

                // Sheets / Posters (type 3)
                new(3,1, "Large reference poster covering Azure architecture patterns and best practices for cloud solutions.", "Azure Architecture Poster", 8.5M, $"{baseUrl}/images/products/9.png"),
                new(3,1, "Quick-reference sheet covering the most important Azure services, use cases, and pricing tiers.", "Azure Services Cheat Sheet", 8.5M, $"{baseUrl}/images/products/10.png"),
                new(3,3, "A5 laminated sheet with the most useful Visual Studio keyboard shortcuts for power users.", "Visual Studio Shortcuts Sheet", 8.5M, $"{baseUrl}/images/products/11.png"),
                new(3,4, "Handy reference sheet with essential SQL Server T-SQL commands, functions, and query patterns.", "SQL Server Commands Sheet", 8.5M, $"{baseUrl}/images/products/12.png"),
                new(3,2, "Quick-reference sheet covering key .NET APIs, LINQ operators, and async/await patterns.", ".NET API Reference Sheet", 8.5M, $"{baseUrl}/images/products/1.png"),
                new(3,2, "Reference sheet illustrating common design patterns implemented in C# and .NET.", ".NET Design Patterns Sheet", 8.5M, $"{baseUrl}/images/products/2.png"),
                new(3,5, "Compact cheat sheet covering C# syntax, features, and language constructs from C# 6 to 12.", "C# Syntax Quick Reference", 8.5M, $"{baseUrl}/images/products/3.png"),
                new(3,5, "Essential Docker CLI commands for building, running, and managing containers in production.", "Docker Commands Sheet", 8.5M, $"{baseUrl}/images/products/4.png"),
                new(3,5, "kubectl commands and Kubernetes concepts reference sheet for cluster administrators.", "Kubernetes Cheat Sheet", 8.5M, $"{baseUrl}/images/products/5.png"),
                new(3,5, "All the Git commands you need for branching, rebasing, stashing, and collaboration workflows.", "Git Commands Sheet", 8.5M, $"{baseUrl}/images/products/6.png"),

                // USB Memory Sticks (type 4)
                new(4,1, "64GB USB 3.0 drive with the Azure logo. Fast, reliable storage for developers on the go.", "Azure USB Drive 64GB", 18M, $"{baseUrl}/images/products/7.png"),
                new(4,1, "128GB high-speed USB drive branded with Azure. Plenty of space for projects and demos.", "Azure USB Drive 128GB", 28M, $"{baseUrl}/images/products/8.png"),
                new(4,3, "32GB USB drive for Visual Studio developers. Pre-loaded with VS Code extensions list.", "Visual Studio USB Drive 32GB", 14M, $"{baseUrl}/images/products/9.png"),
                new(4,4, "64GB USB drive for database professionals. Branded with the SQL Server logo.", "SQL Server USB Drive 64GB", 18M, $"{baseUrl}/images/products/10.png"),
                new(4,2, "32GB USB drive bearing the .NET Foundation logo. A must-have for .NET developers.", ".NET Foundation USB Drive 32GB", 14M, $"{baseUrl}/images/products/11.png"),
                new(4,2, "64GB USB 3.0 drive with the .NET logo in purple. Fast transfers for large project files.", ".NET USB Drive 64GB", 18M, $"{baseUrl}/images/products/12.png"),
                new(4,5, "Sleek 32GB USB drive with developer branding. Compact and pocket-friendly.", "Developer USB Drive 32GB", 14M, $"{baseUrl}/images/products/1.png"),
                new(4,5, "64GB USB drive for DevOps engineers. Carry your scripts and configurations everywhere.", "DevOps USB Drive 64GB", 18M, $"{baseUrl}/images/products/2.png"),
            };
    }
}
