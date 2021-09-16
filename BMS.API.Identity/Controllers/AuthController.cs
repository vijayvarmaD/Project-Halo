using BMS.Infrastructure.Authentication.Models;
using Identity.Helpers;
using Identity.Helpers.Interfaces;
using Identity.Models;
using Identity.Models.InputModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private JwtIssuerOptions _jwtOptions;

        public AuthController(UserManager<AppUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody]UserCredentials userCredentials)
        {
            try
            {
                var identity = await GetClaimsIdentity(userCredentials.UserName, userCredentials.Password);
                if (identity == null)
                {
                    return BadRequest("Invalid username or password.");
                }

                TokenResponse jwt = await Tokens.GenerateJwt(identity, _jwtFactory, userCredentials.UserName, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
                Response.Cookies.Append("jwt", jwt.auth_token, new CookieOptions { HttpOnly = true, Secure = true, IsEssential = true, SameSite = SameSiteMode.Strict });
                return Ok(jwt);
            }
            catch (Exception ex)
            {
                if (ex.Message == "lockout-enabled")
                    return BadRequest("Account Locked out");
                return BadRequest(null);
            }
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null)
            {
                return await Task.FromResult<ClaimsIdentity>(null);
            }

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                // check whether Lockout enabled
                if (await _userManager.GetLockoutEndDateAsync(userToVerify) > DateTimeOffset.UtcNow)
                {
                    throw new Exception("lockout-enabled");
                }
                //await _userManager.SetLockoutEnabledAsync(userToVerify, false);

                // check whether 2FA is enabled
                if (await _userManager.GetTwoFactorEnabledAsync(userToVerify))
                {
                    return await Task.FromResult(_jwtFactory.Generate2FAClaimsIdentity(userName, userToVerify.Id));
                }

                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
    }
}