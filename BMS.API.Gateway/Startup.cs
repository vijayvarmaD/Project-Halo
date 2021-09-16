using BMS.Infrastructure.Authentication.Interfaces;
using BMS.Infrastructure.Authentication.Models;
using BMS.Infrastructure.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace API.Gateway
{
    public class Startup
    {
        private string SecretKey = null;

        private readonly IConfiguration _cfg;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {

            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json",
                    optional: false,
                    reloadOnChange: true)
               .AddEnvironmentVariables();


            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            _cfg = configuration;
        }

        public async void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddControllers();
            //VaultFactory vf = new VaultFactory();
            //SecretKey = await vf.ReadPublicKey();
            //SecretKey = await 
            // secret key
            //SecretKey = _cfg["BMS:API-Gateway:Token-Secret-Key:Public"];


            // cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials());
            });

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            // Secret key
            RsaSecurityKey _signingKey = GenerateRsa(services, _cfg).Result;

            //RsaFactory rsaf = new RsaFactory();
            //RSA publicRsa = rsaf.ReadPublicKeyVault(SecretKey);
            //RsaSecurityKey pubsigningKey = new RsaSecurityKey(publicRsa);
            // Authentication
            services.AddCustomAuth(_cfg, _signingKey);

            services.AddOcelot(_cfg);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = _cfg["PATH_BASE"];

            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            
            app.UseRouting();
            // app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseOcelot().Wait();
        }

        private async Task<RsaSecurityKey> GenerateRsa(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IVaultService, VaultService>();
            services.AddSingleton<IRsaService, RsaService>();
            var sp = services.BuildServiceProvider();
            var vaultService = sp.GetService<IVaultService>();
            SecretKey = await vaultService.ReadPublicKey();

            var rsaService = sp.GetService<IRsaService>();
            RSA publicRsa = rsaService.ReadPublicKey(SecretKey);
            RsaSecurityKey _signingKey = new RsaSecurityKey(publicRsa);
            return _signingKey;
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration configuration, RsaSecurityKey pubsigningKey)
        {
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions =>
            {
                configureOptions.RequireHttpsMetadata = false;
                configureOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.Count > 0)
                        {
                            var token = context.Request.Cookies["jwt"].ToString();
                            context.Request.Headers.Append("Authorization", "Bearer " + token);
                        }
                        return Task.CompletedTask;
                    }
                };
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = pubsigningKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],
                    ValidateAudience = true,
                    ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)]
                };
                configureOptions.SaveToken = true;
            });

            return services;
        }
    }

}
