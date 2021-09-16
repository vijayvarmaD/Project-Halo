using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BMS.Infrastructure.Authorization.Policies
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        private readonly IConfiguration _configuration;

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration) : base(options)
        {
            _options = options.Value;
            _configuration = configuration;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);

            //if (policy == null)
            //{
            //    policy = new AuthorizationPolicyBuilder()
            //        .AddRequirements(new HasScopeRequirement(policyName, $"https://{_configuration["Auth0:Domain"]}/"))
            //        .Build();

            //    // Add policy to the AuthorizationOptions, so we don't have to re-create it each time
            //    _options.AddPolicy(policyName, policy);
            //}

            return policy;
        }
    }
}
