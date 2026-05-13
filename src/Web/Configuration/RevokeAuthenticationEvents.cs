using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.eShopWeb.Web.Configuration;

public class RevokeAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;

    public RevokeAuthenticationEvents(IDistributedCache cache, ILogger<RevokeAuthenticationEvents> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userId = context.Principal?.Claims.First(c => c.Type == ClaimTypes.Name);
        var identityKey = context.Request.Cookies[ConfigureCookieSettings.IdentifierCookieName];

        var cacheKey = $"{userId?.Value}:{identityKey}";
        var revokedEntry = await _cache.GetStringAsync(cacheKey);

        if (revokedEntry != null)
        {
            _logger.LogDebug("Access has been revoked for: {UserId}.", userId?.Value);
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
