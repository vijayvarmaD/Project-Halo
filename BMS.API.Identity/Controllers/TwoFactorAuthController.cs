using BMS.Infrastructure.Authentication.Models;
using Identity.Helpers;
using Identity.Helpers.Interfaces;
using Identity.Models;
using Identity.Models.InputModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/2fa")]
    [ApiController]
    public class TwoFactorAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly ClaimsPrincipal _caller;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;

        public TwoFactorAuthController(UserManager<AppUser> userManager, UrlEncoder urlEncoder, IHttpContextAccessor httpContextAccessor, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
            _caller = httpContextAccessor.HttpContext.User;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

        [Authorize(Policy = "Logged In")]
        [Route("setup")]
        [HttpGet]
        public async Task<IActionResult> TwoFactorSetup()
        {
            // get user
            var user = await _userManager.GetUserAsync(_caller);

            // Generate Authenticator key
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

            // reset authenticator key if there is no key & then generate
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            // get user email id
            var email = user.Email;

            // Format the key to be more readable for the user
            // to be done

            // generate qr code uri string to send to client
            var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

            // QR Code generation            
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10, Color.Black, Color.White, true);
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }

            // return the key & qr img to user
        }

        [Authorize(Policy = "2FA-access")]
        [Route("verify")]
        [HttpPost]
        public async Task<IActionResult> VerifyAuthenticatorKey([FromBody] string verificationCode)
        {
            // get user
            var user = await _userManager.GetUserAsync(_caller);

            // verify if token is valid
            var is2FATokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            // if false
            if (!is2FATokenValid)
            {
                return BadRequest("Invalid otp");
            }

            // set 2fa enabled to true
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            if (await _userManager.CountRecoveryCodesAsync(user) != 0)
            {
                return Ok("Recovery codes are same");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 1);

            return Ok(recoveryCodes);
        }

        [Authorize(Policy = "2FA-access")]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> TwoFactorLogin([FromBody] string verificationCode)
        {
            // get user
            var user = await _userManager.GetUserAsync(_caller);

            // verify if token is valid
            var is2FATokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            // if false
            if (!is2FATokenValid)
            {
                return BadRequest("Invalid otp");
            }

            // create refresh token
            var refToken = await RefreshTokenResetCreate(user);

            // login user
            var identity = await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(user.UserName, user.Id));

            if (identity == null)
            {
                return BadRequest("Invalid username or password.");
            }

            TokenResponse jwt = await Tokens.GenerateJwt(identity, _jwtFactory, user.UserName, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented }, refToken);
            Response.Cookies.Append("jwt", jwt.auth_token, new CookieOptions { HttpOnly = true, Secure = true, IsEssential = true, SameSite = SameSiteMode.Strict });
            Response.Cookies.Append("refresh_token", jwt.refresh_token, new CookieOptions { HttpOnly = true, Secure = true, IsEssential = true, SameSite = SameSiteMode.Strict });
            return Ok(jwt);
        }

        [Authorize(Policy = "IdentityUser")]
        [Route("refresh/renew")]
        [HttpPost]
        public async Task<IActionResult> RenewRefreshToken([FromBody] string refToken)
        {
            // get user
            var user = await _userManager.GetUserAsync(_caller);
            if (await ValidateRefreshToken(user, refToken))
            {
                var newRefToken = await RefreshTokenResetCreate(user);
                return Ok(newRefToken);
            }
            return BadRequest("Invalid Request");
        }

        [Route("access/renew")]
        [HttpPost]
        public async Task<IActionResult> RenewAccessToken([FromBody] TokenRenewal tokens)
        {
            var res = _jwtFactory.GetPrincipalFromExpiredToken(tokens.AccessToken);
            return Ok("Hi");
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticationUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issue={0}&digits=6";

            return string.Format(AuthenticationUriFormat, _urlEncoder.Encode("BMS Project Halo"), _urlEncoder.Encode(email), unformattedKey);
        }

        private async Task<string> RefreshTokenResetCreate(AppUser user)
        {
            // check for the expired refresh token user sent & delete it
            await _userManager.RemoveAuthenticationTokenAsync(user, "BMS", "RefreshToken");

            // generate a token with required properties
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(user, "BMS", "RefreshToken");

            // assign it to user
            await _userManager.SetAuthenticationTokenAsync(user, "BMS", "RefreshToken", newRefreshToken);
            return newRefreshToken;
        }

        private async Task<bool> ValidateRefreshToken(AppUser user, string userRefreshToken)
        {
            return await _userManager.VerifyUserTokenAsync(user, "BMS", "RefreshToken", userRefreshToken);
        }
    }
}