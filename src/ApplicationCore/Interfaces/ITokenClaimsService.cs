using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Issues authentication tokens for authenticated users.</summary>
public interface ITokenClaimsService
{
    /// <summary>Returns a signed JWT for the given user, including their roles as claims.</summary>
    Task<string> GetTokenAsync(string userName);
}
