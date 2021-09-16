using BMS.Infrastructure.Authentication.Models;
using Identity.Helpers.Interfaces;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Helpers
{
    public class Tokens
    {
        public static async Task<TokenResponse> GenerateJwt(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings, string refreshToken = null)
        {
            var response = new TokenResponse()
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = await jwtFactory.GenerateEncodedToken(userName, identity),
                refresh_token = refreshToken,
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds
            };
            //var response = await jwtFactory.GenerateEncodedToken(userName, identity);
            //return response;
            //return JsonConvert.SerializeObject(response, serializerSettings);
            return response;
            
        }
    }
}
