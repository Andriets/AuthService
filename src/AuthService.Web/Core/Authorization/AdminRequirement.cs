using AuthService.Web.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Web.Core.Authorization;

public class AdminRequirement : IAuthorizationRequirement;

public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        var isSuperAdmin = context.User.HasClaim(JwtClaimConstants.IsSuperAdmin, "true");
        var isAdmin = context.User.IsInRole("Admin");

        if (isSuperAdmin || isAdmin)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
