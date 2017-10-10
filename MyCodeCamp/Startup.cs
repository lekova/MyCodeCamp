using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCodeCamp.DbUtilities;
using MyCodeCamp.Entities;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyCodeCamp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IHostingEnvironment env;

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
                //.AddIdentity<CampUser, IdentityRole>();

            services.AddIdentity<CampUser, IdentityRole>()
                .AddEntityFrameworkStores<CampContext>();

            services.AddAuthentication().AddCookie(options => {
                options.Cookie.Expiration = TimeSpan.FromDays(14);
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "MyCookieName";
                options.LoginPath = "/api/auth/login";
                options.AccessDeniedPath = "/api/auth/forbidden";
            });
            
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "MyCookieName";
                options.LoginPath = "/api/auth/login";
                options.AccessDeniedPath = "/api/auth/forbidden";
            });

            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddTransient<CampIdentityInitializer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

            services.AddCors(config => 
            {
                config.AddPolicy("LinasPolicy", bldr =>
                {
                    bldr.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://linalekova.com");
                });
                config.AddPolicy("AnyGet", bldr =>
                {
                    bldr.AllowAnyHeader()
                        .WithMethods("GET")
                        .AllowAnyOrigin();
                });
            });

            services.AddMvc(opt =>
            {
                if(!env.IsProduction())
                {
                    opt.SslPort = 44355;
                }
                opt.Filters.Add(new RequireHttpsAttribute());
            })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = 
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app
                            , IHostingEnvironment env
                            , CampDbInitializer dbSeeder
                            , CampIdentityInitializer identitySeeder
                            , ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // this is a global config, if we want to config separately we need to use polices
            app.UseCors(config =>
            {
                //config.AllowAnyHeader()
                //.AllowAnyMethod()
                //.WithOrigins("http://linalekova.com");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
            dbSeeder.Seed().Wait();
            identitySeeder.Seed().Wait();
        }
    }
}
