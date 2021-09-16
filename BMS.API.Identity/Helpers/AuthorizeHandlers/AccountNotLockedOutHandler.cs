using Identity.Helpers.AuthorizeRequirements;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Identity.Helpers.AuthorizeHandlers
{
    public class AccountNotLockedOutHandler : AuthorizationHandler<AccountNotLockedOutRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountNotLockedOutRequirement requirement)
        {
            var user = context.User;
            var claim = context.User.FindFirst("lockout-enabled");
            if (claim != null)
            {
                if (claim?.Value == "true")
                {
                    context.Fail();
                }
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
