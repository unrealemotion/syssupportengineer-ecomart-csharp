# JOIEnergy Project: Refactoring Documentation (Post-BugFix)

This document details the refactoring steps applied to the JOIEnergy project *after* the initial bug-fixing phase. The refactoring aims to improve code structure, maintainability, testability, and best practices adherence, *without* altering core functionality.

## Section 1: Areas Identified for Refactoring

The "BugFixed" version, while functionally correct, had several design and architectural weaknesses:

1.  **Tight Coupling and Lack of Abstraction (Services):**
    *   `InMemoryMeterReadingService`, `AccountService`, and `PricePlanService` were concrete classes with hardcoded dependencies (in-memory dictionaries, lists). This hindered:
        *   Unit testing in isolation (mocking was difficult).
        *   Switching to a different storage mechanism (e.g., a database).
        *   Extending or modifying service behavior.

2.  **Hardcoded Configuration (Startup):**
    *   Price plans, smart meter mappings, and initial meter readings were hardcoded in the `Startup` class, reducing flexibility and requiring recompilation for configuration changes.

3.  **Overly Complex `StoreReadings` Method (InMemoryMeterReadingService):**
    *   The `StoreReadings` method combined input validation, data manipulation (add/update), and final validation, violating the Single Responsibility Principle and increasing complexity.

4.  **Inconsistent Controller Response (MeterReadingController):**
    *   The `MeterReadingController` returned a `BadRequestObjectResult` for both validation errors *and* successful updates, providing unclear feedback to the API consumer. Also, the `GetReading` didn't do input validation.

5. **Direct usage of** `DateTime.Now`:
    * This made unit testing time-dependent behavior very difficult.

6. **Unnecessary `ToList()`**
    * Some LINQ queries include unnecessary `ToList()` which affect performance.

7. **Unused interface**:
 * `Debug` interface is not used.

## Section 2: Refactoring Steps - Detailed Explanation ("How")

The refactoring process was implemented through these key steps:

1.  **Introduce Interfaces:**
    *   Defined interfaces (`IAccountService`, `IMeterReadingService`, `IPricePlanService`, `IPricePlanStore`, `IDateTimeProvider`) to establish contracts for service components, decoupling them and enabling dependency injection.

2.  **Implement Dependency Injection (DI):**
    *   Modified `PricePlanComparatorController` and `PricePlanService` to accept their dependencies (interfaces) through constructor injection.
    *   Refactored `Startup` to use the .NET Core dependency injection container, registering interfaces with their concrete implementations.

3.  **Refactor `InMemoryMeterReadingService`:**
    *   Extracted validation logic from `StoreReadings` into separate private methods (`ValidateNewReadings`, `ValidateCombinedReadings, CheckForUpdates`).
    *   Created a `CombineReadings` method to handle merging existing and new readings (with deep copies to avoid unintended side effects).
    *   Made `StoreReadings` a coordinator, orchestrating validation and data manipulation.

4.  **Improve `MeterReadingController` Logic:**
    *   Modified the `Post` method to return distinct `OkObjectResult` responses for successful additions and updates, providing informative messages.
    *   Added comprehensive input validation (null checks, etc.).
     * Validated input for method `GetReading`.

5.  **Refactor `Startup`:**
    * Removed hardcoded data (price plans, account mappings, readings); these are now managed by `InMemoryPricePlanStore`.
    *  Register service using interfaces.

6.  **Introduce** `IDateTimeProvider` **and its implementation**.
     *  Created interface `IDateTimeProvider`, and class `DateTimeProvider`.

7. **Remove unnecessary `ToList()`**
    *  Get rid of unnecessary `ToList` method.

8. **Remove unused** `Debug` **interface.**

## Section 3: Code Comparison (Original vs. Refactored)

This section provides a step-by-step code comparison, showing the original ("BugFixed") code alongside the refactored code.

### 3.1. `Startup.cs`

**Original (`Startup.cs` - relevant parts):**

```csharp
// Original Startup.cs (BugFixed version - relevant parts)
using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            var readings = GenerateMeterElectricityReadings();
            var pricePlans = new List<PricePlan> {
                new PricePlan{
                    EnergySupplier = Enums.Supplier.DrEvilsDarkEnergy,
                    UnitRate = 10m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.TheGreenEco,
                    UnitRate = 2m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                },
                new PricePlan{
                    EnergySupplier = Enums.Supplier.PowerForEveryone,
                    UnitRate = 1m,
                    PeakTimeMultiplier = new List<PeakTimeMultiplier>()
                }
            };

            services.AddMvc(options => options.EnableEndpointRouting = false); // Adds MVC services to the application.
            services.AddTransient<IAccountService, AccountService>(); // Registers AccountService as a transient service.
            services.AddTransient<IMeterReadingService, MeterReadingService>(); // Registers MeterReadingService as a transient service.
            services.AddTransient<IPricePlanService, PricePlanService>(); // Registers PricePlanService as a transient service.
            services.AddSingleton((IServiceProvider arg) => readings); // Registers the generated readings as a singleton.
            services.AddSingleton((IServiceProvider arg) => pricePlans); // Registers the price plans as a singleton.
            services.AddSingleton((IServiceProvider arg) => SmartMeterToPricePlanAccounts); // Registers the smart meter to price plan accounts as a singleton.
        }

        // This method gets called by the runtime. Use this method to
        // configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Enables the developer exception page for debugging in development.
            }

            app.UseMvc(); // Adds MVC middleware to the request pipeline.
        }

        // Generates sample meter electricity readings for testing purposes.
        // Returns: A dictionary mapping smart meter IDs to lists of electricity readings.
        private Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            var generator = new ElectricityReadingGenerator();
            var smartMeterIds = SmartMeterToPricePlanAccounts.Select(mtpp => mtpp.Key);
            foreach (var smartMeterId in smartMeterIds)
            {
                readings.Add(smartMeterId, generator.Generate(20));
            }
            return readings;
        }

        // Gets the hardcoded smart meter to price plan account mappings.
        // Returns: A dictionary mapping smart meter IDs to supplier enums.
        public Dictionary<String, Supplier> SmartMeterToPricePlanAccounts
        {
            get
            {
                Dictionary<String, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
                smartMeterToPricePlanAccounts.Add("smart-meter-0", Supplier.DrEvilsDarkEnergy);
                smartMeterToPricePlanAccounts.Add("smart-meter-1", Supplier.TheGreenEco);
                smartMeterToPricePlanAccounts.Add("smart-meter-2", Supplier.DrEvilsDarkEnergy);
                smartMeterToPricePlanAccounts.Add("smart-meter-3", Supplier.PowerForEveryone);
                smartMeterToPricePlanAccounts.Add("smart-meter-4", Supplier.TheGreenEco);
                return smartMeterToPricePlanAccounts;
            }
        }
         private static List<PeakTimeMultiplier> NoMultipliers()
        {
            return new List<PeakTimeMultiplier>();
        }
    }
}
```

**Refactored (`Startup.cs` - using Dependency Injection):**

```csharp
// Refactored Startup.cs (using Dependency Injection)

using JOIEnergy.Domain;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;   // Add this for IHostEnvironment


namespace JOIEnergy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Register the service
            services.AddSingleton<IPricePlanStore, InMemoryPricePlanStore>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();          
            services.AddSingleton<IMeterReadingService,InMemoryMeterReadingService>();
            services.AddSingleton<IAccountService, InMemoryAccountService>();
            services.AddScoped<IPricePlanService,PricePlanService>();  // PricePlanService is now scoped.
            services.AddMvc(options => options.EnableEndpointRouting = false);

        }


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
```

### 3.2 IDateTimeProvider
```csharp
//IDateTimeProvider.cs
using System;
namespace JOIEnergy.Services
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
```
### 3.3 DateTimeProvider
```csharp
//DateTimeProvider.cs
using System;
namespace JOIEnergy.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
```

### 3.4.  `IPricePlanStore` Interface (NEW)

```csharp
// IPricePlanStore.cs (NEW)
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using System.Collections.Generic;

namespace JOIEnergy.Services
{
    public interface IPricePlanStore
    {
        List<PricePlan> GetPricePlans();

        Dictionary<string, Supplier> GetSmartMeterToPricePlanAccounts();
        public Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings();
    }
}

```
### 3.5 InMemoryPricePlanStore
```csharp

// InMemoryPricePlanStore.cs
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
namespace JOIEnergy.Services
{
    public class InMemoryPricePlanStore : IPricePlanStore
    {
          private readonly IDateTimeProvider _dateTimeProvider;
           public InMemoryPricePlanStore(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        public List<PricePlan> GetPricePlans()
        {
          
            // Hardcoded price plans (for now - would come from config/DB)
            return new List<PricePlan>() {
                new PricePlan() { EnergySupplier = Supplier.DrEvilsDarkEnergy, UnitRate = 10, PeakTimeMultiplier = NoMultipliers() },
                new PricePlan() { EnergySupplier = Supplier.TheGreenEco, UnitRate = 2, PeakTimeMultiplier = NoMultipliers() },
                new PricePlan() { EnergySupplier = Supplier.PowerForEveryone, UnitRate = 1, PeakTimeMultiplier = NoMultipliers() }
            };
        }
         private  List<PeakTimeMultiplier> NoMultipliers()
        {
            return new List<PeakTimeMultiplier>();
        }

        public Dictionary<string, Supplier> GetSmartMeterToPricePlanAccounts()
        {
            // Hardcoded account mappings (for now - would come from config/DB)
            Dictionary<String, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
            smartMeterToPricePlanAccounts.Add("smart-meter-0", Supplier.DrEvilsDarkEnergy);
            smartMeterToPricePlanAccounts.Add("smart-meter-1", Supplier.TheGreenEco);
            smartMeterToPricePlanAccounts.Add("smart-meter-2", Supplier.DrEvilsDarkEnergy);
            smartMeterToPricePlanAccounts.Add("smart-meter-3", Supplier.PowerForEveryone);
            smartMeterToPricePlanAccounts.Add("smart-meter-4", Supplier.TheGreenEco);
            return smartMeterToPricePlanAccounts;
        }

          public  Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings() {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            var generator = new ElectricityReadingGenerator();
            var smartMeterIds =  GetSmartMeterToPricePlanAccounts().Keys;
            foreach (var smartMeterId in smartMeterIds)
            {
                readings.Add(smartMeterId, generator.Generate(20));
            }
            return readings;
        }      
        
    }
}

```
### 3.6. `IAccountService` Interface (Exists - No Change)

```csharp
// IAccountService.cs (Exists - No Change)
// (This file already existed in the BugFixed version and doesn't change)
using JOIEnergy.Enums;
using System.Collections.Generic;
namespace JOIEnergy.Services
{
    public interface IAccountService
    {
        Supplier GetPricePlanIdForSmartMeterId(string smartMeterId);
         IEnumerable<string> GetSmartMeterIds(); // Added to support loading readings
    }
}

```

### 3.7 `InMemoryAccountService.cs` (Refactored from `AccountService.cs`)

```csharp
// Services/AccountService.cs
// This service manages the association between smart meters and their assigned energy suppliers (price plans).

using System;
using System.Collections.Generic;
using JOIEnergy.Enums;
using System.Linq;
namespace JOIEnergy.Services
{
    // Implements IAccountService and inherits from Dictionary for convenience (though this could be refactored).
    public class InMemoryAccountService : IAccountService
    {
        private Dictionary<string, Supplier> _smartMeterToPricePlanAccounts;

        // Constructor: Initializes the service with a dictionary of smart meter to price plan mappings.
        // Parameters:
        //   smartMeterToPricePlanAccounts: A dictionary where the key is the smart meter ID (string)
        //                                   and the value is the corresponding Supplier enum.
        public InMemoryAccountService(IPricePlanStore pricePlanStore)
        {
            _smartMeterToPricePlanAccounts = pricePlanStore.GetSmartMeterToPricePlanAccounts();
        }

        // Retrieves the price plan (Supplier) associated with a given smart meter ID.
        // Parameters:
        //   smartMeterId: The ID of the smart meter (string).
        // Returns:
        //   The Supplier enum representing the associated price plan.  Returns Supplier.NullSupplier if no mapping is found.
        public Supplier GetPricePlanIdForSmartMeterId(string smartMeterId)
        {
            if (!_smartMeterToPricePlanAccounts.ContainsKey(smartMeterId))
            {
                return Supplier.NullSupplier;
            }

            return _smartMeterToPricePlanAccounts[smartMeterId];
        }
        public IEnumerable<string> GetSmartMeterIds()
        {
            return _smartMeterToPricePlanAccounts.Keys.ToList();
        }
    }
}

```

### 3.8. `IMeterReadingService` Interface (Exists - No Change)
```csharp
//IMeterReadingService
using System.Collections.Generic;
using JOIEnergy.Domain;
namespace JOIEnergy.Services
{
    public interface IMeterReadingService
    {
        List<ElectricityReading> GetReadings(string smartMeterId);
        string StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings);
    }
}
```
### 3.9. `InMemoryMeterReadingService.cs` (REFACTORED - Separated Validation and Logic)

```csharp
// InMemoryMeterReadingService.cs (REFACTORED - Separated Validation and Logic)
using JOIEnergy.Domain;
using System.Collections.Generic;
using System;
using System.Linq;

namespace JOIEnergy.Services
{
    public class InMemoryMeterReadingService : IMeterReadingService
    {
        private readonly Dictionary<string, List<ElectricityReading>> _meterReadings = new Dictionary<string, List<ElectricityReading>>();

        public List<ElectricityReading> GetReadings(string smartMeterId)
        {
            // Use TryGetValue for efficient lookup and null handling.
            if (_meterReadings.TryGetValue(smartMeterId, out var readings))
            {
                return readings.OrderBy(r => r.Time).ToList();  // Return a new, sorted list.
            }
            return new List<ElectricityReading>();
        }

        public string StoreReadings(string smartMeterId, List<ElectricityReading> newReadings)
        {
            // 1. Input Validation (separate method)
            string validationError = ValidateNewReadings(newReadings);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // 2. Get Existing Readings (if any)
            if (!_meterReadings.TryGetValue(smartMeterId, out List<ElectricityReading> existingReadings))
            {
                existingReadings = new List<ElectricityReading>();
                _meterReadings[smartMeterId] = existingReadings;  // Add to dictionary if it doesn't exist.
            }

            // 3. Combine and Sort (create a DEEP COPY)
            List<ElectricityReading> combinedReadings = CombineReadings(existingReadings, newReadings);


            // 4.  Validate Combined Readings
            validationError = ValidateCombinedReadings(combinedReadings);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // 5. Store (if all validations pass)
            _meterReadings[smartMeterId] = combinedReadings;


            return CheckForUpdates(existingReadings,newReadings) ? "Reading(s) updated." : string.Empty;
        }
       
        private string ValidateNewReadings(List<ElectricityReading> newReadings)
        {
            if (newReadings == null || !newReadings.Any())
            {
                return "Cannot store an empty list of readings.";
            }

            if (newReadings.Any(r => r.Time == default))
            {
                return "Cannot store a reading with a default Time value.";
            }
            //check duplicate in new readings
             if (newReadings.GroupBy(r => r.Time).Any(g => g.Count() > 1))
            {
                return "Input readings contain duplicate timestamps.";
            }


            return string.Empty;
        }
        //Validate the combined readings
        private string ValidateCombinedReadings(List<ElectricityReading> combinedReadings)
        {
           
            for (int i = 0; i < combinedReadings.Count - 1; i++)
            {
                if (combinedReadings[i].Reading > combinedReadings[i + 1].Reading)
                {
                    return $"Readings are not monotonically non-decreasing (time: {combinedReadings[i].Time}).";
                }
            }
            return string.Empty;
        }
        //check for update
        private bool CheckForUpdates(List<ElectricityReading> existingReadings, List<ElectricityReading> newReadings)
        {
            if (existingReadings == null || existingReadings.Count == 0) return false;

            // Check if any new readings have the same timestamp as an existing reading
            return newReadings.Any(newReading => existingReadings.Any(existingReading => existingReading.Time == newReading.Time));
        }

        //Combine, update and sort.
       private List<ElectricityReading> CombineReadings(List<ElectricityReading> existingReadings, List<ElectricityReading> newReadings)
        {
            // Create a deep copy to avoid modifying the original list or its elements.
            List<ElectricityReading> combined = existingReadings.Select(r => new ElectricityReading { Time = r.Time, Reading = r.Reading }).ToList();

            foreach (var newReading in newReadings)
            {
                // Find the reading (if existed) in the *copied* list.
                var existingReading = combined.FirstOrDefault(x => x.Time == newReading.Time);

                if (existingReading != null)
                {
                    // Update the *copy*, not the original.
                    existingReading.Reading = newReading.Reading;
                }
                else
                {
                    // Add a *copy* of the new reading.
                    combined.Add(new ElectricityReading { Time = newReading.Time, Reading = newReading.Reading });
                }
            }
            return combined.OrderBy(r => r.Time).ToList();
        }
    }
}

```

### 3.10.  `IPricePlanService` Interface (Exists - No Change)

```csharp
// IPricePlanService.cs (Exists - No Change)
// (This file already existed in the BugFixed version and doesn't change)

using System.Collections.Generic;

namespace JOIEnergy.Services
{
    public interface IPricePlanService
    {
        Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId);
    }
}

```

### 3.11. `PricePlanService.cs` (REFACTORED - Dependency Injection)

```csharp
// PricePlanService.cs (REFACTORED - Dependency Injection)
using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        private readonly IMeterReadingService _meterReadingService;
        private readonly IPricePlanStore _pricePlanStore; // Inject IPricePlanStore
        private readonly IDateTimeProvider _dateTimeProvider;
        public PricePlanService(IMeterReadingService meterReadingService, IPricePlanStore pricePlanStore, IDateTimeProvider dateTimeProvider)
        {
            _meterReadingService = meterReadingService;
            _pricePlanStore = pricePlanStore;   // Assign injected dependency
            _dateTimeProvider = dateTimeProvider;
        }

        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
           if (pricePlan == null) throw new ArgumentNullException("Price plan cannot be null.",nameof(pricePlan));
            decimal totalCost = 0;
            // Iterate through consecutive pairs of readings.
            //sort by time
             electricityReadings = electricityReadings.OrderBy(x=>x.Time).ToList();
            for (int i = 0; i < electricityReadings.Count - 1; i++)
            {
                // Calculate the energy consumed during this interval.
                decimal energyConsumed = electricityReadings[i + 1].Reading - electricityReadings[i].Reading;
                 if(energyConsumed < 0)
                {
                    throw new ArgumentException($"Meter reading {i+1} cannot have reading less than the previous one, please check your data",nameof(electricityReadings));
                }
                // Use the GetPrice method of PricePlan to get correct price based on the reading time
                totalCost += pricePlan.GetPrice(electricityReadings[i].Time) * energyConsumed;
            }
            return totalCost;
        }

        public Dictionary<String, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(String smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);
            if (electricityReadings == null || !electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }
            //load from the store
            var pricePlans = _pricePlanStore.GetPricePlans();

            return pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(electricityReadings, plan));
        }
    }
}
```

### 3.12 `MeterReadingController.cs` (REFACTORED - Improved Response Handling)
```csharp
// MeterReadingController.cs (REFACTORED - Improved Response Handling)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Domain;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;

namespace JOIEnergy.Controllers
{
    [Route("readings")]
    public class MeterReadingController : Controller
    {
        private readonly IMeterReadingService _meterReadingService;

        public MeterReadingController(IMeterReadingService meterReadingService)
        {
            _meterReadingService = meterReadingService;
        }

        [HttpPost("store")]
        public ObjectResult Post([FromBody] MeterReadings meterReadings)
        {
            // Input validation
             if (meterReadings == null)
            {
                return new BadRequestObjectResult("Invalid request - null");
            }
            if (meterReadings.ElectricityReadings == null)
             {
                return new BadRequestObjectResult("Invalid request - null");
            }
             foreach (var reading in meterReadings.ElectricityReadings)
            {
                if (reading == null)
                {
                    return new BadRequestObjectResult("Invalid request - null");
                }
            }
             if (!IsMeterReadingsValid(meterReadings)) //Keep the method, it checks if smartMeterId is not empty
            {
                return new BadRequestObjectResult("Internal Server Error");
            }

            string result = _meterReadingService.StoreReadings(meterReadings.SmartMeterId, meterReadings.ElectricityReadings);

            if (string.IsNullOrEmpty(result))
            {
                return new OkObjectResult(new { message = "Readings stored successfully." }); // Success - new readings added
            }
            else if (result == "Reading(s) updated.")
            {
                return new OkObjectResult(new { message = result }); // Success - readings updated.
            }
            else
            {
                return new BadRequestObjectResult(result); // Return the validation error message
            }
        }

        private bool IsMeterReadingsValid(MeterReadings meterReadings)
        {

            string smartMeterId = meterReadings.SmartMeterId;
            List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;
            // FIXED: Corrected logic to check for non-null and non-empty.
            return smartMeterId != null && smartMeterId.Any()
                && electricityReadings != null && electricityReadings.Any();
        }

        [HttpGet("read/{smartMeterId}")]
        public ObjectResult GetReading(string smartMeterId)
        {
           if (string.IsNullOrEmpty(smartMeterId))
            {
                return new BadRequestObjectResult("Smart meter ID cannot be null or empty");
            }
            return new OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
        }
    }
}
```

