using BMS.API.Movies.Models;
using BMS.API.Movies.Models.Interfaces;
using BMS.API.Movies.Services;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace API.Movies
{
    public class Startup
    {
        private string SecretKeyPub = null;

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json",
                     optional: false,
                     reloadOnChange: true)
                .AddEnvironmentVariables();


            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
                IdentityModelEventSource.ShowPII = true;
            }

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //SecretKeyPub = Configuration["BMS:API.Movies:Token-Secret-Key:Public"];         
            
            services.AddDatabase(Configuration);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials());
            });
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<MovieInfoService>();
            // Secret key
            RsaSecurityKey _signingKey = GenerateRsa(services, Configuration).Result;

            //RsaFactory rsaf = new RsaFactory();
            //RSA publicRsa = rsaf.ReadPublicKey(SecretKeyPub);
            //RsaSecurityKey _pubKey = new RsaSecurityKey(publicRsa);

            // Authentication
            services.AddCustomAuth(Configuration, _signingKey);

            // Authorization Policies
            services.AddPolicies();

            // Identity Setup
            //services.AddCustomIdentity();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task<RsaSecurityKey> GenerateRsa(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IVaultService, VaultService>();
            services.AddSingleton<IRsaService, RsaService>();
            var sp = services.BuildServiceProvider();
            var vaultService = sp.GetService<IVaultService>();
            SecretKeyPub = await vaultService.ReadPublicKey();

            var rsaService = sp.GetService<IRsaService>();
            RSA publicRsa = rsaService.ReadPublicKey(SecretKeyPub);
            RsaSecurityKey _signingKey = new RsaSecurityKey(publicRsa);
            return _signingKey;
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration configuration, RsaSecurityKey _pubKey)
        {
            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            //// Configure JwtIssuerOptions
            //services.Configure<JwtIssuerOptions>(options =>
            //{
            //    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
            //    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
            //    options.SigningCredentials = new SigningCredentials(_pubKey, SecurityAlgorithms.RsaSha256);
            //});

            // Configure token validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _pubKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // add auth to pipeline
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.IncludeErrorDetails = true;
                configureOptions.SaveToken = true;
            });

            return services;
        }

        //public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        //{
        //    services.AddIdentityCore<AppUser>(options =>
        //    {
        //        options.ClaimsIdentity.UserIdClaimType = "id";
        //    })
        //      .AddDefaultTokenProviders()
        //      .AddTokenProvider("BMS", typeof(DataProtectorTokenProvider<AppUser>));

        //    return services;
        //}

        public static IServiceCollection AddPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Identity global policy
                options.AddPolicy("Logged In", p =>
                {
                    p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireAuthenticatedUser();
                });

                //// All Microservices - global - all users
                //options.AddPolicy("allmicroservices-global-allusers", p =>
                //{
                //    p.Requirements.Add(new AccountNotLockedOutRequirement(false));
                //});

                //// 2FA Authentication access policy
                //options.AddPolicy("2FA-access", p =>
                //{
                //    p.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.TwoFactorAccess);
                //});
            });
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MovieDatabaseSettings>(configuration.GetSection(nameof(MovieDatabaseSettings)));
            services.AddSingleton<IMovieDatabaseSettings>(x => x.GetRequiredService<IOptions<MovieDatabaseSettings>>().Value);
            
            return services;
        }
    }
}
