using BMS.Infrastructure.Authentication.Models;
using Identity.Helpers.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Helpers
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        public JwtFactory(IOptions<JwtIssuerOptions> jwtIssuerOptions)
        {
            _jwtIssuerOptions = jwtIssuerOptions.Value;
            ThrowIfInvalidOptions(_jwtIssuerOptions);
        }

        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity)
        {
            try
            {
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Jti, await _jwtIssuerOptions.JtiGenerator()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtIssuerOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                    identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Rol),
                    identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id)
                };


                // Create the JWT security token
                var jwt = new JwtSecurityToken(
                    issuer: _jwtIssuerOptions.Issuer,
                    audience: _jwtIssuerOptions.Audience,
                    claims: claims,
                    notBefore: _jwtIssuerOptions.NotBefore,
                    expires: _jwtIssuerOptions.Expiration,
                    signingCredentials: _jwtIssuerOptions.SigningCredentials
                );

                //// TEST CODE
                //var tokenDescriptor = new SecurityTokenDescriptor
                //{
                //    Audience = _jwtIssuerOptions.Audience,
                //    Issuer = _jwtIssuerOptions.Issuer,
                //    Subject = new ClaimsIdentity(claims),
                //    NotBefore = _jwtIssuerOptions.NotBefore,
                //    Expires = _jwtIssuerOptions.Expiration,
                //    SigningCredentials = _jwtIssuerOptions.SigningCredentials
                //};


                // Encode JWT token
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                return encodedJwt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ClaimsIdentity GenerateClaimsIdentity(string userName, string id)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Constants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.ApiAccess)
            });
        }

        public ClaimsIdentity Generate2FAClaimsIdentity(string userName, string id)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Constants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.TwoFactorAccess)
            });
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                // Hide this in secret manager
                string SecretKey = "iNivDmHLpUA223sqshgdtfkhjbkdRj1PVkH"; // todo: get this from somewhere secure
                SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
                // Hide this in secret manager

                // remove bearer in front of token
                string actToken = token.Substring(8);

                var tokenValidationParamters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,
                    ValidateLifetime = false
                };


                var tknhndlr = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tknhndlr.ValidateToken(actToken, tokenValidationParamters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");
                return principal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // move to token.cs
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity, string privateKey)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtIssuerOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtIssuerOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Rol),
                identity.FindFirst(Constants.Strings.JwtClaimIdentifiers.Id)
            };


            // Create the JWT security token
            var jwt = new JwtSecurityToken(
                issuer: _jwtIssuerOptions.Issuer,
                audience: _jwtIssuerOptions.Audience,
                claims: claims,
                notBefore: _jwtIssuerOptions.NotBefore,
                expires: _jwtIssuerOptions.Expiration,
                signingCredentials: _jwtIssuerOptions.SigningCredentials
            );

            // Encode JWT token
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}
