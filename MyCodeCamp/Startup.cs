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

namespace MyCodeCamp
{
    public class Startup
    {
        private IConfigurationRoot config;
        private IHostingEnvironment env;

        public Startup(IHostingEnvironment env)//IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
            this.env = env;
            config = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(config);
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped)
                .AddIdentity<CampUser, IdentityRole>();
            
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

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
        public void Configure(IApplicationBuilder app,      IHostingEnvironment env, 
                              CampDbInitializer dbSeeder,   ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(config.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            dbSeeder.Seed().Wait();
        }
    }
}
