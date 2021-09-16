using Microsoft.AspNetCore.Authorization;

namespace Identity.Helpers.AuthorizeRequirements
{
    public class AccountNotLockedOutRequirement : IAuthorizationRequirement
    {
        public bool AccountNotLockedOut { get; set; }

        public AccountNotLockedOutRequirement(bool noLockout)
        {
            AccountNotLockedOut = noLockout;
        }
    }
}
