using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserListEndpoint(UserManager<ApplicationUser> userManager):EndpointWithoutRequest<UserListResponse>
{

    public override void Configure()
    {
        Get("api/users");
        Roles(ApplicationCore.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d => d.Produces<UserListResponse>()
        .WithTags("UserManagementEndpoints"));
    }

    public override async Task<UserListResponse> ExecuteAsync(CancellationToken ct)
    {
        var response = new UserListResponse();
        var users = userManager.Users.ToList();
        foreach ( var user in users)
        {
            response.Users.Add(user.ToUserDto());
        }
        return response;
    }
}
