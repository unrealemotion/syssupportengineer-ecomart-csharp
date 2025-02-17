//Startup.cs
using JOIEnergy.Controllers;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
namespace JOIEnergy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add
        // services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Register the service
            services.AddSingleton<IPricePlanStore, InMemoryPricePlanStore>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IMeterReadingService, InMemoryMeterReadingService>();
            services.AddSingleton<IAccountService, InMemoryAccountService>();
            services.AddScoped<IPricePlanService, PricePlanService>();
            services.AddMvc(options => options.EnableEndpointRouting = false);

        }
        // This method gets called by the runtime. Use this method to
        // configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using JOIEnergy.Domain;
//using JOIEnergy.Enums;
//using JOIEnergy.Generator;
//using JOIEnergy.Services;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.DependencyInjection;

//namespace JOIEnergy
//{

//    // Startup.cs
//    // This class configures the ASP.NET Core application. It sets up services,
//    // dependency injection, and the request processing pipeline.
//    public class Startup
//    {
//        private const string MOST_EVIL_PRICE_PLAN_ID = "price-plan-0";
//        private const string RENEWABLES_PRICE_PLAN_ID = "price-plan-1";
//        private const string STANDARD_PRICE_PLAN_ID = "price-plan-2";
//        // Constructor: Initializes the Startup class with configuration settings.
//        // Parameters:
//        //   configuration: An IConfiguration instance representing the application's configuration.
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        // Gets the application's configuration.
//        public IConfiguration Configuration { get; }

//        // Configures the services used by the application. This method is called by the runtime.
//        // Parameters:
//        //   services: An IServiceCollection instance to register services with.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            var readings =
//                 GenerateMeterElectricityReadings();

//            var pricePlans = new List<PricePlan> {
//                new PricePlan{
//                    EnergySupplier = Enums.Supplier.DrEvilsDarkEnergy,
//                    UnitRate = 10m,
//                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
//                },
//                new PricePlan{
//                    EnergySupplier = Enums.Supplier.TheGreenEco,
//                    UnitRate = 2m,
//                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
//                },
//                new PricePlan{
//                    EnergySupplier = Enums.Supplier.PowerForEveryone,
//                    UnitRate = 1m,
//                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
//                }
//            };

//            // Add MVC services to the container.  Disabling endpoint routing for compatibility with older routing style.
//            services.AddMvc(options => options.EnableEndpointRouting = false);
//            // Register services for dependency injection.
//            services.AddTransient<IAccountService, AccountService>();
//            services.AddTransient<IMeterReadingService, MeterReadingService>();
//            services.AddTransient<IPricePlanService, PricePlanService>();
//            services.AddSingleton((IServiceProvider arg) => readings);
//            services.AddSingleton((IServiceProvider arg) => pricePlans);
//            services.AddSingleton((IServiceProvider arg) => SmartMeterToPricePlanAccounts);
//        }

//        // Configures the HTTP request pipeline. This method is called by the runtime.
//        // Parameters:
//        //   app: An IApplicationBuilder instance to configure the application's request pipeline.
//        //   env: An IWebHostEnvironment instance providing information about the hosting environment..
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage(); // Enables the developer exception page for debugging in development.
//            }

//            app.UseMvc(); // Adds MVC middleware to the request pipeline.
//        }

//        // Generates sample meter electricity readings for testing purposes.
//        // Returns: A dictionary mapping smart meter IDs to lists of electricity readings.
//        private Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings()
//        {
//            var readings = new Dictionary<string, List<ElectricityReading>>();
//            var generator = new ElectricityReadingGenerator();
//            var smartMeterIds = SmartMeterToPricePlanAccounts.Select(mtpp => mtpp.Key);

//            foreach (var smartMeterId in smartMeterIds)
//            {
//                readings.Add(smartMeterId, generator.Generate(20));
//            }
//            return readings;
//        }

//        // Gets the hardcoded smart meter to price plan account mappings.
//        // Returns: A dictionary mapping smart meter IDs to supplier enums.
//        public Dictionary<String, Supplier> SmartMeterToPricePlanAccounts
//        {
//            get
//            {
//                Dictionary<String, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
//                smartMeterToPricePlanAccounts.Add("smart-meter-0", Supplier.DrEvilsDarkEnergy);
//                smartMeterToPricePlanAccounts.Add("smart-meter-1", Supplier.TheGreenEco);
//                smartMeterToPricePlanAccounts.Add("smart-meter-2", Supplier.DrEvilsDarkEnergy);
//                smartMeterToPricePlanAccounts.Add("smart-meter-3", Supplier.PowerForEveryone);
//                smartMeterToPricePlanAccounts.Add("smart-meter-4", Supplier.TheGreenEco);
//                return smartMeterToPricePlanAccounts;
//            }
//        }
//    }
//}
