using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using poc.Google.Directions.Interfaces;
using poc.Google.Directions.Models;
using poc.Google.Directions.Services;

namespace poc.Google.Directions
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnv)
        {
            Configuration = configuration;
            ApiSettings = new ApiSettings
            {
                GoogleApiKey = Configuration["GoogleApiKey"]
            };

            WebHostEnvironment = hostingEnv;
        }

        public ApiSettings ApiSettings { get; init; }
        public IConfiguration Configuration { get; }
        internal IWebHostEnvironment WebHostEnvironment { get; init; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);
            RegisterHttpClients(services);

            var mvcBuilder = services.AddRazorPages();

            if (WebHostEnvironment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }
            else
            {
                services.AddHsts(options =>
                {
                    options.MaxAge = TimeSpan.FromDays(365);
                    options.IncludeSubDomains = true;
                    options.Preload = true;
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        protected virtual void RegisterHttpClients(IServiceCollection services)
        {
            //services.AddHttpClient<IDirectionsService, DirectionsService>();
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(ApiSettings);
            services.AddTransient<IDirectionsService, DirectionsService>();
            services.AddTransient<IMagicLinkService, MagicLinkService>();
            services.AddTransient<IProviderDataService, ProviderDataService>();
        }
    }
}
