using System;
using System.Net;
using System.Net.Http;
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
                GoogleDirectionsApiKey = Configuration["GoogleDirectionsApiKey"],
                GooglePlacesApiKey = Configuration["GooglePlacesApiKey"]
            };

            WebHostEnvironment = hostingEnv;
        }

        public ApiSettings ApiSettings { get; init; }
        public IConfiguration Configuration { get; }
        internal IWebHostEnvironment WebHostEnvironment { get; init; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterHttpClients(services);
            RegisterServices(services);

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

            services.AddMemoryCache();
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
            services.AddHttpClient<IPostcodeLookupService, PostcodeLookupService>();

            services.AddHttpClient<IDirectionsService, DirectionsService>(
                    nameof(DirectionsService),
                    client => { }
                )
                .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    var handler = new HttpClientHandler();

                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }
                    return handler;
                });

            services.AddHttpClient<IPlacesService, PlacesService>(
                    nameof(PlacesService),
                    client => { }
                )
                .ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    var handler = new HttpClientHandler();

                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    }
                    return handler;
                });
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(ApiSettings);
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IDirectionsService, DirectionsService>();
            services.AddTransient<IMagicLinkService, MagicLinkService>();
            services.AddTransient<IPlacesService, PlacesService>();
            services.AddTransient<IPostcodeLookupService, PostcodeLookupService>();
            services.AddTransient<IProviderDataService, ProviderDataService>();
        }
    }
}
