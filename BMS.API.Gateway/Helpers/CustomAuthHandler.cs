using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.Gateway.Helpers
{
    internal class CustomAuthHandler : AuthenticationHandler<CustomAuthOptions>
    {
        private HttpContext context;
        public CustomAuthHandler(IOptionsMonitor<CustomAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, HttpContext _context) : base(options, logger, encoder, clock)
        {
            context = _context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            context.Request.Headers.Add("Authorization", "Bearer " + context.Request.Cookies["jwt"].ToString());
            var res = await AuthenticateAsync();
            return AuthenticateResult.NoResult();
        }
    }
}
