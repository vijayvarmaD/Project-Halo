using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Helpers.Interfaces
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id);
        ClaimsIdentity Generate2FAClaimsIdentity(string userName, string id);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
