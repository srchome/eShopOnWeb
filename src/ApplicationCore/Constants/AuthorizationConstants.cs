namespace Microsoft.eShopWeb.ApplicationCore.Constants;

public class AuthorizationConstants
{
    public static string AUTH_KEY { get; set; } = string.Empty;

    // Default password for seeding - should be changed on first login in production
    public static string DEFAULT_PASSWORD { get; set; } = "Pass@word1";

    public static string JWT_SECRET_KEY { get; set; } = string.Empty;
}
