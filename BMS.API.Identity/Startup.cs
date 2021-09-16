using AutoMapper;
using BMS.Infrastructure.Authentication.Interfaces;
using BMS.Infrastructure.Authentication.Models;
using BMS.Infrastructure.Authentication.Services;
using Identity.Data;
using Identity.Helpers;
using Identity.Helpers.AuthorizeHandlers;
using Identity.Helpers.AuthorizeRequirements;
using Identity.Helpers.Interfaces;
using Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Identity
{
    public class Startup
    {
        //private const string SecretKey = "iNivDmHLpUA223sqshgdtfkhjbkdRj1PVkH"; // todo: get this from somewhere secure
        private string SecretKey = null;
        private string SecretKeyPub = null;
        //private static SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public Startup(IConfiguration configuration, IHostingEnvironment env)
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
            IdentityModelEventSource.ShowPII = true;
            // secret key
            //SecretKey = Configuration["BMS:Identity:Token-Secret-Key:Private"];
            //SecretKeyPub = Configuration["BMS:Identity:Token-Secret-Key:Public"];
            //RSA publicRsa = rsaf.ReadPublicKey(SecretKeyPub);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials());
            });

            // Application DBContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("localdb"),
                    b => b.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name
                ));
            });
            services.AddSingleton<IAuthorizationHandler, AccountNotLockedOutHandler>();
            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            RsaSecurityKey _signingKey = GenerateRsa(services, Configuration).Result;                     

            // Authentications
            services.AddCustomAuth(Configuration, _signingKey);

            // Authorization Policies
            services.AddPolicies();

            // Identity Setup
            services.AddCustomIdentity();
            //services.AddIdentityCore<AppUser>(options => {
            //    options.ClaimsIdentity.UserIdClaimType = "id";
            //}).AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders()
            //.AddTokenProvider("BMS", typeof(DataProtectorTokenProvider<AppUser>));

            // Automapper
            services.AddAutoMapper(typeof(Startup));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMvc();
        }

        private async Task<RsaSecurityKey> GenerateRsa(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IVaultService, VaultService>();
            services.AddSingleton<IRsaService, RsaService>();
            var sp = services.BuildServiceProvider();
            var vaultService = sp.GetService<IVaultService>();
            SecretKey = await vaultService.ReadPrivateKey();

            var rsaService = sp.GetService<IRsaService>();
            RSA privateRsa = rsaService.ReadPrivateKey(SecretKey);
            RsaSecurityKey _signingKey = new RsaSecurityKey(privateRsa);
            return _signingKey;
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration configuration, RsaSecurityKey _signingKey)
        {
            //// TEST CODE 
            //string SecretKeyPub = configuration["BMS:Identity:Token-Secret-Key:Public"];
            //RsaFactory rsaf = new RsaFactory();
            //RSA publicRsa = rsaf.ReadPublicKey(SecretKeyPub);
            //RsaSecurityKey pubsigningKey = new RsaSecurityKey(publicRsa);


            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);
            });

            // Configure token validation parameters
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // add auth to pipeline
            services.AddAuthentication(options =>
            {
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions =>
            {
                configureOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.Count > 0 && string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
                        {
                            var token = context.Request.Cookies["jwt"].ToString();
                            context.Request.Headers.Append("Authorization", "Bearer " + token);
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        return Task.CompletedTask;
                    }

                };
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.IncludeErrorDetails = true;
                configureOptions.SaveToken = true;
            });

            // Google Authentication - External Provider -> WIP
            //if (configuration["Authentication:Google:ClientId"] != null)
            //{
            //    services.AddAuthentication().AddGoogle(o =>
            //    {
            //        o.ClientId = configuration["Authentication:Google:ClientId"];
            //        o.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            //    });
            //}
            return services;
        }

        public static IServiceCollection AddPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // 
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .Build();

                // Identity global policy
                options.AddPolicy("IdentityUser", p =>
                {
                    p.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess);
                });

                // All Microservices - global - all users
                options.AddPolicy("allmicroservices-global-allusers", p =>
                {
                    p.Requirements.Add(new AccountNotLockedOutRequirement(false));
                });

                // 2FA Authentication access policy
                options.AddPolicy("2FA-access", p =>
                {
                    p.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.TwoFactorAccess);
                });

                options.AddPolicy("Logged In", p =>
                {
                    p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireAuthenticatedUser();
                });
            });
            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<AppUser>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = "id";
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders()
              .AddTokenProvider("BMS", typeof(DataProtectorTokenProvider<AppUser>));

            return services;
        }
    }
}
