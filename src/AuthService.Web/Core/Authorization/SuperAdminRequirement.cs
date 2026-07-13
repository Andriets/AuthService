using AuthService.Web.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Web.Core.Authorization;

public class SuperAdminRequirement : IAuthorizationRequirement;

public class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SuperAdminRequirement requirement)
    {
        if (context.User.HasClaim(JwtClaimConstants.IsSuperAdmin, "true"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
