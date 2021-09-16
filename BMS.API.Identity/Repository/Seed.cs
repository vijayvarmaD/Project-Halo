using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Repository
{
    public class Seed
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public Seed(UserManager<AppUser> userManager, ApplicationDbContext appDbContext, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _roleManager = roleManager;
        }

        //public async Task<bool> RolesandClaimsSetup()
        //{
        //    List<RolesandClaims> defaultRolesAndClaims = new List<RolesandClaims>
        //    {
        //        new RolesandClaims { RoleName = "Admin", Claims = { new Claim(ClaimTypes.Role, "Admin") } },
        //        new RolesandClaims { RoleName = "PermissionsManager", Claims = { new Claim(ClaimTypes)} }
        //    };



        //    // Admin
        //    var role = await _roleManager.FindByNameAsync("Admin");
        //    if (role == null)
        //    {
        //        role = new IdentityRole("Admin");
        //        await _roleManager.CreateAsync(role);
        //        await _roleManager.AddClaimAsync(role, new Claim(ClaimTypes.Role,"Admin"));
        //    }

        //    // Permissions Manager
        //    role = await _roleManager.FindByNameAsync("PermissionsManager");
        //    if (role == null)
        //    {

        //    }
        //}
    }
}
