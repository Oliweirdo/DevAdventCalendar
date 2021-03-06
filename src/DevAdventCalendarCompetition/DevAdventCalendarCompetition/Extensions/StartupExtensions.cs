using System;
using System.Globalization;
using DevAdventCalendarCompetition.Repository;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevAdventCalendarCompetition.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
                    {
                        config.SignIn.RequireConfirmedEmail = true;
                    })
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IBaseTestRepository, BaseTestRepository>();
            services.AddTransient<IHomeRepository, HomeRepository>();

            services.AddScoped<DbInitializer>();

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmailSender, EmailSender>(
                sender =>
                {
                    var emailSender = new EmailSender
                    {
                        Host = configuration.GetValue<string>("Email:Smtp:Host"),
                        Port = configuration.GetValue<int>("Email:Smtp:Port"),
                        UserName = configuration.GetValue<string>("Email:Smtp:UserName"),
                        Password = configuration.GetValue<string>("Email:Smtp:Password")
                    };

                    return emailSender;
                });

            services.AddTransient<INotificationService, EmailNotificationService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IBaseTestService, BaseTestService>();
            services.AddTransient<IHomeService, HomeService>();
            services.AddTransient<IManageService, ManageService>();
            services.AddTransient<IdentityService>();
            return services;
        }

        public static IServiceCollection AddExternalLoginProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
            })
            .AddTwitter(twitterOptions =>
            {
                twitterOptions.ConsumerKey = configuration["Authentication:Twitter:ConsumerKey"];
                twitterOptions.ConsumerSecret = configuration["Authentication:Twitter:ConsumerSecret"];
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            })
            .AddGitHub(githubOptions =>
            {
                githubOptions.ClientId = configuration["Authentication:GitHub:ClientId"];
                githubOptions.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];
                githubOptions.Scope.Add("user:email");
            });

            return services;
        }

        public static void UpdateDatabase(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var init = scope.ServiceProvider.GetService<DbInitializer>();
                init.Seed();
            }
        }

        public static void UseHttpsRequestScheme(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use(next => context =>
                {
                    context.Request.Scheme = "https";
                    return next(context);
                });
        }

        public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("pl-PL")
                };

                options.DefaultRequestCulture = new RequestCulture("pl-PL");
                options.SupportedCultures = supportedCultures;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<EmailNotificationOptions>(configuration.GetSection("EmailNotification"));

            return services;
        }
    }
}