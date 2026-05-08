using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using UptimeMonitor.Core.Plugins;
using UptimeMonitor.Service.MonitorService.Extensions;
using UptimeMonitor.Service.MonitorService.Monitor;
using UptimeMonitor.Web.Api.Auth;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Api.Services;
using UptimeMonitor.Web.Api.Swagger;
using UptimeMonitor.Web.Auth;
using UptimeMonitor.Web.Config;
using UptimeMonitor.Web.Data;
using UptimeMonitor.Web.Services;
using UptimeMonitor.Web.Swagger;

namespace UptimeMonitor.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);
            ConfigureServices(builder);

            var app = builder.Build();
            ConfigureMiddleware(app);
            app.Run();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();

            builder.Services.AddSerilog();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddPluginLoader();

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                options.ForwardLimit = null;

                // Replace with IP of your proxy/load balancer
                options.KnownProxies.Clear();

                // 192.168.1.0/24 allows any from 192.168.1.1-254;
                options.KnownNetworks.Clear();
            });

            builder.Services.AddSecurityHeaderPolicies()
              .SetPolicySelector((PolicySelectorContext ctx) =>
              {
                  return SecurityHeadersDefinitions.GetHeaderPolicyCollection(
                      builder.Environment.IsDevelopment(), builder.Configuration.GetValue<string>("Oidc:Authority"));
              });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "__Host-X-XSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.Strict;
                if (!builder.Environment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            });

            builder.Services.AddAutoMapper((config) => { }, Assembly.GetExecutingAssembly());

            builder.Services.AddDbContextFactory<MonitorDbContext>(config =>
            {
                config.UseNpgsql(builder.Configuration.GetConnectionString("MonitorDbContext"));
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("admin-v1", new OpenApiInfo { Title = "UptimeMonitor Admin API", Version = "v1" });
                options.SwaggerDoc("public-v1", new OpenApiInfo { Title = "UptimeMonitor Public API", Version = "v1" });
                options.SwaggerDoc("worker-v1", new OpenApiInfo { Title = "UptimeMonitor Worker API", Version = "v1" });

                options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
                options.SupportNonNullableReferenceTypes();

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (docName.StartsWith("admin-"))
                    {
                        return apiDesc.GroupName == "Admin";
                    }
                    else if (docName.StartsWith("public-"))
                    {
                        return apiDesc.GroupName == "Public";
                    }
                    else if (docName.StartsWith("worker-"))
                    {
                        return apiDesc.GroupName == "Worker";
                    }
                    return false;
                });
            });

            builder.Services.Configure<WorkerAuthOptions>(builder.Configuration.GetSection("WorkerAuth"));
            builder.Services.AddScoped<WorkerTokenService>();
            builder.Services.AddScoped<HeartbeatService>();
            builder.Services.AddScoped<WorkerService>();
            builder.Services.AddHostedService<CleanupBackgroundService>();

            ConfigureAuthentication(builder);
            ConfigureAuthorization(builder);

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<MonitorDbContext>(
                    "db_health_check",
                    customTestQuery: (dbContext, token) => Task.FromResult(dbContext.Database.CanConnect())
                );

            builder.Services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new ApiExplorerHideNonApiControllersConvention());
            });

            builder.Services.AddRazorPages();
            builder.Services.AddReverseProxy()
               .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            var internalWorkerConfiguration = builder.Configuration.GetSection("InternalWorker");
            var internalWorkerEnabled = internalWorkerConfiguration.GetValue<bool>("Enabled");
            if (internalWorkerEnabled)
            {
                builder.Services.AddScoped<IWorkerService, InternalMonitorService>();
                builder.Services.AddMonitorService();
            }
        }

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                // If handling an API Request, redirect to 401
                options.Events.OnRedirectToIdentityProvider = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api"))
                    {
                        if (ctx.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }

                        ctx.HandleResponse();
                    }

                    return Task.CompletedTask;
                };

                options.Authority = builder.Configuration.GetValue<string>("Oidc:Authority");
                options.ClientId = builder.Configuration.GetValue<string>("Oidc:ClientId");
                options.ClientSecret = builder.Configuration.GetValue<string>("Oidc:ClientSecret");

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.TokenValidationParameters.NameClaimType = builder.Configuration.GetValue("Oidc:NameClaimType", ClaimTypes.Name);

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("groups");
            })
            .AddJwtBearer("WorkerAuth", options =>
            {
                var tokenSecret = builder.Configuration.GetValue<string>("WorkerAuth:Secret");
                var tokenIssuer = builder.Configuration.GetValue<string>("WorkerAuth:Issuer");
                var tokenAudience = builder.Configuration.GetValue<string>("WorkerAuth:Audience");

                if (string.IsNullOrWhiteSpace(tokenSecret) || string.IsNullOrWhiteSpace(tokenIssuer) || string.IsNullOrWhiteSpace(tokenAudience))
                {
                    throw new Exception("Worker Token settings are not setup correctly.");
                }

                var key = Encoding.UTF8.GetBytes(tokenSecret);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenIssuer,
                    ValidAudience = tokenAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        }

        private static void ConfigureAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("WorkerOnly", policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes("WorkerAuth"));
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseForwardedHeaders();

            // Todo: move this to external dbmigrator project
            app.EnsureMigrationOfContext<MonitorDbContext>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("admin-v1/swagger.json", "Admin API V1");
                    c.SwaggerEndpoint("worker-v1/swagger.json", "Worker API V1");
                    c.SwaggerEndpoint("public-v1/swagger.json", "Public API V1");
                });

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSecurityHeaders();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/healthz")
                .AllowAnonymous()
                .DisableAntiforgery();

            app.MapRazorPages();
            app.MapControllers();
            app.MapNotFound("/api/{**segment}");

            if (app.Environment.IsDevelopment())
            {
                var uiDevServer = app.Configuration.GetValue<string>("UiDevServerUrl");
                if (!string.IsNullOrEmpty(uiDevServer))
                {
                    app.MapReverseProxy();
                }
            }

            app.MapFallbackToPage("/_Host");
        }
    }
}