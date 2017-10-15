﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voidwell.DaybreakGames.Census;
using Newtonsoft.Json;
using Voidwell.DaybreakGames.Data;
using Voidwell.DaybreakGames.Services.Planetside;
using Voidwell.DaybreakGames.Websocket;
using Newtonsoft.Json.Serialization;
using Voidwell.Cache;
using Voidwell.DaybreakGames.CensusServices;

namespace Voidwell.DaybreakGames
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddDataAnnotations()
                .AddJsonFormatters(options =>
                {
                    options.NullValueHandling = NullValueHandling.Ignore;
                    options.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddCache("DaybreakGames");
            services.AddEntityFrameworkContext();
            services.AddCensusClient(Configuration.GetSection("Census"));
            services.AddCensusServices();

            services.AddOptions();
            services.Configure<DaybreakAPIOptions>(Configuration);
            services.AddTransient(a => a.GetRequiredService<IOptions<DaybreakAPIOptions>>().Value);

            services.AddTransient<IConfiguration>(sp => Configuration);

            services.AddSingleton<ICharacterService, CharacterService>();
            services.AddSingleton<IOutfitService, OutfitService>();
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IMapService, MapService>();
            services.AddSingleton<IFactionService, FactionService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<ITitleService, TitleService>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<IWorldService, WorldService>();
            services.AddSingleton<IZoneService, ZoneService>();
            services.AddSingleton<IWeaponService, WeaponService>();
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<ICombatReportService, CombatReportService>();
            services.AddSingleton<IMetagameEventService, MetagameEventService>();
            services.AddSingleton<IWorldMonitor, WorldMonitor>();
            services.AddSingleton<IUpdaterService, UpdaterService>();
            services.AddSingleton<IWebsocketEventHandler, WebsocketEventHandler>();
            services.AddSingleton<IWebsocketMonitor, WebsocketMonitor>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Error }
                })
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug();

            app.UseMvc();
        }
    }
}
