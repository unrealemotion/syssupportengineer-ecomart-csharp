**JOIEnergy Project: Refactoring Documentation - BugFixed to BugFixed-Refactored**

This document details the refactoring steps taken to improve the JOIEnergy project, moving from the "BugFixed" state to the "BugFixed-Refactored" state. It focuses on key areas of improvement, explains the rationale behind each change, provides step-by-step instructions (where applicable), and shows code comparisons.

**Part 1: Summary of Inefficiencies and Improvements**

This refactoring focused primarily on improving *testability*, *maintainability*, *robustness*, *clarity* and *data integrity*.  The original "BugFixed" code had several inefficiencies:

1.  **Tight Coupling:** Classes were directly dependent on concrete implementations, making unit testing extremely difficult and hindering future modifications.
2.  **Lack of Abstraction:**  The absence of interfaces made it hard to swap out implementations (e.g., for testing or using different data sources).
3.  **Missing Dependency Injection:**  Dependencies were created *within* classes, rather than being provided externally, reinforcing tight coupling.
4.  **Inadequate Input Validation:** The "BugFixed" version had minimal input validation, increasing the risk of runtime errors.
5.  **Mutability and Data Integrity Risks:** In-memory data structures were directly modified and exposed, potentially leading to unintended side effects and data corruption.
6. Code Redundancy: some part of code does the same thing, those actions could be extracted to a new method or class.
7. **Lack of contract**: Some of services miss interfaces to define clear contract.

**Part 2: Detailed Descriptions of Inefficiencies and Solutions (with Code Snippets)**

This section goes into detail on each identified inefficiency, explaining the problem and showing the code *before* the refactoring.

**2.1 Tight Coupling and Lack of Abstraction (Controllers)**

*   **Problem:** The `MeterReadingController` and `PricePlanComparatorController` were directly dependent on concrete service classes (e.g., `MeterReadingService`, `AccountService`, `PricePlanService`).  This made it impossible to test the controllers in isolation without also testing the services, a violation of unit testing principles.

*   **Example (Before - `PricePlanComparatorController` constructor):**

    ```csharp
    // Controllers/PricePlanComparatorController.cs
    public class PricePlanComparatorController : Controller
    {
    private readonly IPricePlanService _pricePlanService;
    private readonly IAccountService _accountService;
    public PricePlanComparatorController(IPricePlanService
    pricePlanService, IAccountService accountService)
    {
    this._pricePlanService = pricePlanService;
    this._accountService = accountService;
    }
    // ...
    }
    ```
    This code snippet used to contain Newtondsoft.Json.Linq, which would be removed.
    This code snippet showed direct dependencies.

**2.2 Missing Dependency Injection (Controllers and Startup)**

*   **Problem:**  The `Startup` class directly instantiated concrete service classes and passed them to the controllers.  This made it difficult to configure different implementations (e.g., using mock objects for testing) without modifying the `Startup` class itself.
*   **Example (Before - `Startup.cs` `ConfigureServices` method):**

    ```csharp
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

    // public class Startup
    // {

        // public Startup(IConfiguration configuration)
        // {
            // Configuration = configuration;
        // }
    // public IConfiguration Configuration { get; }
    // public void ConfigureServices(IServiceCollection services)
        // {
            // var readings =
            // GenerateMeterElectricityReadings();
            // var pricePlans = new List<PricePlan> {
                // new PricePlan{
                    // EnergySupplier =
    // Enums.Supplier.DrEvilsDarkEnergy,
                    // UnitRate = 10m,
                    // PeakTimeMultiplier = new
    // List<PeakTimeMultiplier>()
                // },
                // new PricePlan{
                    // EnergySupplier = Enums.Supplier.TheGreenEco,
                    // UnitRate = 2m,
                    // PeakTimeMultiplier = new
    // List<PeakTimeMultiplier>()
                // },
                // new PricePlan{
                    // EnergySupplier =
    // Enums.Supplier.PowerForEveryone,
                    // UnitRate = 1m,
                    // PeakTimeMultiplier = new
    // List<PeakTimeMultiplier>()
                // }
            // };
    // services.AddMvc(options =>
    // options.EnableEndpointRouting = false);
    // services.AddTransient<IAccountService,
    // AccountService>();
    // services.AddTransient<IMeterReadingService,
    // MeterReadingService>();
    // services.AddTransient<IPricePlanService,
    // PricePlanService>();
            // services.AddSingleton((IServiceProvider arg) =>
    // readings);
            // services.AddSingleton((IServiceProvider arg) =>
    // pricePlans);
            // services.AddSingleton((IServiceProvider arg) =>
    // SmartMeterToPricePlanAccounts);
        // }
    ```

    This shows the *direct instantiation* of service classes and hardcoded data.  This setup is inflexible.

**2.3 Inadequate Input Validation (`MeterReadingController`)**

*   **Problem:**  The `MeterReadingController.Post` method (for storing readings) had minimal validation. While it checked for a `null` `MeterReadings` object, it didn't thoroughly validate the contents of that object, or handle invalid data gracefully.
*   **Example (Before - `MeterReadingController.Post`):**
    ```csharp
        [HttpPost("store")]
        public ObjectResult Post([FromBody] MeterReadings meterReadings)
        {
        //Comprehensive null validation is still needed for this endpoint.
            if (meterReadings == null)
            {
                return new BadRequestObjectResult("Invalid request - null");
            }
        //moved down
            if (!IsMeterReadingsValid(meterReadings))
            {
                return new BadRequestObjectResult("Internal Server Error");
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
        string result =
        _meterReadingService.StoreReadings(meterReadings.SmartMeterId,
        meterReadings.ElectricityReadings);
            if (string.IsNullOrEmpty(result))
            {
                return new OkObjectResult("Readings stored successfully."); //
            }
        }
    ```
    This snippet check null for the object itself, but lacks validation of smartMeterId, time, and reading values within each `ElectricityReading`.

**2.4 Mutability and Data Integrity Risks (`InMemoryMeterReadingService`)**

*   **Problem:** The original `InMemoryMeterReadingService.StoreReadings` method directly modified the internal `_meterReadings` dictionary and the lists it contained.  This meant that external code could potentially alter the stored data, leading to unexpected behavior and data corruption.  It also lacked proper synchronization, making it unsafe for concurrent access.

*   **Example (Before - `InMemoryMeterReadingService.StoreReadings` - *Conceptual Snippet*):**

    ```csharp
    // Simplified conceptual representation of the original flawed logic
    public void StoreReadings(string smartMeterId, List<ElectricityReading> newReadings) {
        if (!_meterReadings.ContainsKey(smartMeterId)) {
            _meterReadings[smartMeterId] = new List<ElectricityReading>();
        }

        // Directly adding to the stored list - PROBLEM!
        _meterReadings[smartMeterId].AddRange(newReadings);
    }
    ```

    The `AddRange` method directly adds the *incoming* `newReadings` list to the stored list.  If the caller later modifies `newReadings`, the stored data is also modified.

**2.5. Lack of contract (`PricePlanService`, `AccountService`,`MeterReadingService`)**
* **Problem**: the original `PricePlanService`, `AccountService`,`MeterReadingService` are concrete class with no interface, this will make difficult to change when business needs

**Part 3: Refactoring Steps and Solutions (with Code Comparisons)**

This section details the solutions implemented, showing the *before* and *after* code for each change.

**3.1 Implementing Dependency Injection and Inversion of Control**

*   **Step 1: Create Interfaces:**  Define interfaces for each service: `IMeterReadingService`, `IPricePlanService`, `IAccountService`, `IPricePlanStore`, and `IDateTimeProvider`.  These interfaces specify the *contracts* that the concrete implementations must fulfill.

    ```csharp
    // IMeterReadingService.cs
    public interface IMeterReadingService
    {
        List<ElectricityReading> GetReadings(string smartMeterId);
        string StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings);
    }
    ```
    (Similar interfaces are created for `IPricePlanService`, `IAccountService`, `IPricePlanStore`, and 'IDateTimeProvider`).

*   **Step 2: Modify Controllers to Use Interfaces:** Update the controllers to accept *interfaces* in their constructors, rather than concrete classes.

    ```csharp
    // MeterReadingController.cs (Refactored)
    public class MeterReadingController : Controller
    {
        private readonly IMeterReadingService _meterReadingService;

        public MeterReadingController(IMeterReadingService meterReadingService)
        {
            _meterReadingService = meterReadingService;
        }
        // ... rest of the controller ...
    }
    ```

    ```csharp
    // PricePlanComparatorController.cs (Refactored)
    //Refactored
    namespace JOIEnergy.Controllers
    {
        [Route("price-plans")]
        public class PricePlanComparatorController : Controller
        {
            private readonly IPricePlanService _pricePlanService;
            private readonly IAccountService _accountService;
            private readonly IMeterReadingService _meterReadingService; // Inject MeterReading Service
                                                                        // Use constructor injection for
                                                                        // dependencies
            public PricePlanComparatorController(IPricePlanService pricePlanService,
            IAccountService accountService, IMeterReadingService meterReadingService)
            {
                _pricePlanService = pricePlanService;
                _accountService = accountService;
                _meterReadingService = meterReadingService; //Inject MeterReadingService
            }
        //...
        }
    }
    ```

*   **Step 3: Configure Dependency Injection in `Startup.cs`:**  Use the `ConfigureServices` method to register the interfaces and their corresponding concrete implementations with the .NET Core dependency injection container.

    ```csharp
    // Startup.cs (Refactored ConfigureServices)
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
    ```

    *   `AddSingleton`: Creates a single instance of the service for the entire application lifetime (appropriate for `InMemory...` services).
    *   `AddScoped`: Creates a new instance of the service for each HTTP request (appropriate for `PricePlanService` as it depends on scoped services).

**3.2 Implementing In-Memory Data Stores with Interfaces**

* **Step 1: Create In-memory Implementations**
Create the `InMemoryPricePlanStore`, `InMemoryMeterReadingService` and `InMemoryAccountService`.

```csharp
    //InMemoryPricePlanStore.cs
    public class InMemoryPricePlanStore : IPricePlanStore
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public InMemoryPricePlanStore(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
    // ...
    }

```

```csharp
    //InMemoryAccountService.cs
    public class InMemoryAccountService : IAccountService
    {
        private readonly Dictionary<string, Supplier>
    _smartMeterToPricePlanAccounts;
    public InMemoryAccountService(IPricePlanStore pricePlanStore)
    {
        _smartMeterToPricePlanAccounts =
    pricePlanStore.GetSmartMeterToPricePlanAccounts();
    }
    // ...
    }
```
*   **Step 2: Remove the Original Services:** Since the `InMemory...` classes now handle both the service logic and the data storage, the original `MeterReadingService`, `PricePlanService`, and `AccountService` classes are removed.

**3.3 Enhanced Input Validation (`MeterReadingController` and `InMemoryMeterReadingService`)**

*   **Step 1:  Improve `IsMeterReadingsValid`:**

    ```csharp
    // MeterReadingController.cs (Refactored IsMeterReadingsValid)
    private bool IsMeterReadingsValid(MeterReadings meterReadings)
    {
        // Check if meterReadings itself is null
        if (meterReadings == null)
        {
            return false;
        }
        // Directly check properties of meterReadings for null and emptiness
        return meterReadings.SmartMeterId != null
            && meterReadings.SmartMeterId.Any()
            && meterReadings.ElectricityReadings != null
            && meterReadings.ElectricityReadings.Any();
    }

    ```
    Now we check `meterReadings` not be null
    `meterReadings.SmartMeterId` not be null or empty
    and `meterReadings.ElectricityReadings` not be null or empty.

*   **Step 2:  Add Comprehensive Validation in `InMemoryMeterReadingService`:**  Create new private methods within `InMemoryMeterReadingService` to handle validation: `ValidateNewReadings` and `ValidateCombinedReadings`.

    ```csharp
    // InMemoryMeterReadingService.cs (New Validation Methods)

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
    ```

    *   `ValidateNewReadings`: Checks for null/empty input, default `Time` values, and duplicate timestamps *within* the new readings.
    *   `ValidateCombinedReadings`:  Ensures that the combined readings (existing + new) are monotonically non-decreasing (i.e., readings don't go down over time).

*   **Step 3: Integrate Validation into `StoreReadings`:**

    ```csharp
        public string StoreReadings(string smartMeterId,
        List<ElectricityReading> newReadings)
        {
        // 1. Input Validation (separate method)
        string validationError = ValidateNewReadings(newReadings);
        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }
        // 2. Get Existing Readings (if any)
        if (!_meterReadings.TryGetValue(smartMeterId, out
        List<ElectricityReading> existingReadings))
        {
            existingReadings = new List<ElectricityReading>();
            _meterReadings[smartMeterId] = existingReadings;
        }
        // 3. Combine and Sort (create a DEEP COPY)
        List<ElectricityReading> combinedReadings =
        CombineReadings(existingReadings, newReadings);
        // 4. Validate Combined Readings
        validationError = ValidateCombinedReadings(combinedReadings);
        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }
        // 5. Store (if all validations pass)
        _meterReadings[smartMeterId] = combinedReadings;
        return CheckForUpdates(existingReadings, newReadings) ? "Reading(s)
    updated." : string.Empty;
        }
    ```

    The `StoreReadings` method now:
    1.  Validates the new readings.
    2.  Retrieves existing readings (if any).
    3.  Combines and sorts the readings (using the deep-copying `CombineReadings` method, described below).
    4.  Validates the *combined* readings.
    5.  Only stores the data if *all* validations pass.
    6. Call a new method to check if there is an update.

* **Step4: Create `CheckForUpdates` method**
```csharp
    //check for update
    private bool CheckForUpdates(List<ElectricityReading>
    existingReadings, List<ElectricityReading> newReadings)
    {
        if (existingReadings == null || existingReadings.Count == 0) return
    false;
    // Check if any new readings have the same timestamp as an existing
    reading
        return newReadings.Any(newReading =>
    existingReadings.Any(existingReading => existingReading.Time ==
    newReading.Time));
    }
```

**3.4 Implementing Deep Copying and Immutability (`InMemoryMeterReadingService`)**

*   **Create `CombineReadings` method:**  This new method is responsible for combining existing and new readings *without* modifying the original lists.

    ```csharp
    //Combine, update and sort.
    private List<ElectricityReading>
    CombineReadings(List<ElectricityReading> existingReadings,
    List<ElectricityReading> newReadings)
    {
    // Create a deep copy to avoid modifying the original list or its
    elements.
    List<ElectricityReading> combined = existingReadings.Select(r => new
    ElectricityReading { Time = r.Time, Reading = r.Reading }).ToList();
    foreach (var newReading in newReadings)
    {
    // Find the reading (if existed) in the *copied* list.
    var existingReading = combined.FirstOrDefault(x => x.Time ==
    newReading.Time);
        if (existingReading != null)
        {
        // Update the *copy*, not the original.
        existingReading.Reading = newReading.Reading;
        }
        else
        {
        // Add a *copy* of the new reading.
        combined.Add(new ElectricityReading { Time = newReading.Time, Reading
        = newReading.Reading });
        }
    }
        return combined.OrderBy(r => r.Time).ToList();
    }
    ```

    *   **Deep Copy:** `existingReadings.Select(...).ToList()` creates a *new* list containing *new* `ElectricityReading` objects.  Changes to `combined` do *not* affect `existingReadings`.
    *   **Copy-on-Write:**  Existing readings are updated by modifying the *copied* object.  New readings are added as *new* objects.
    *  **Returns New List**: always return a new sorted list.
    *   **Immutability:** The original `_meterReadings` dictionary and the lists it contains are never directly modified by external code.  The `GetReadings` method also returns a *new* sorted list, further protecting the internal data.

**3.5. Refactor PricePlanService calculation logic**
* **Create `calculateCost` method:** 
```csharp
    private decimal calculateCost(List<ElectricityReading>
    electricityReadings, PricePlan pricePlan)
    {
        if (pricePlan == null) throw new ArgumentNullException("Price plan
    cannot be null.", nameof(pricePlan));
    decimal totalCost = 0;
    // Iterate through consecutive pairs of readings.
    //sort by time
    electricityReadings = electricityReadings.OrderBy(x =>
    x.Time).ToList();
        for (int i = 0; i < electricityReadings.Count - 1; i++)
        {
        // Calculate the energy consumed during this interval.
        decimal energyConsumed = electricityReadings[i + 1].Reading -
    electricityReadings[i].Reading;
            if (energyConsumed < 0)
            {
            throw new ArgumentException($"Meter reading {i + 1} cannot have
    reading less than the previous one, please check your data",
    nameof(electricityReadings));
            }
        // Use the GetPrice method of PricePlan to get correct price based on
    the reading time
        totalCost += pricePlan.GetPrice(electricityReadings[i].Time) *
    energyConsumed;
        }
    return totalCost;
    }
```
This method sort by time, get price and throw exception if the input is invalid
*   **Refactor `GetConsumptionCostOfElectricityReadingsForEachPricePlan` method:**  
```csharp
    public Dictionary<string, decimal>
    GetConsumptionCostOfElectricityReadingsForEachPricePlan(string
    smartMeterId)
    {
        List<ElectricityReading> electricityReadings =
        _meterReadingService.GetReadings(smartMeterId);
        if (electricityReadings == null || !electricityReadings.Any())
        {
            return new Dictionary<string, decimal>();
        }
        //load from the store
        var pricePlans = _pricePlanStore.GetPricePlans();
            return pricePlans.ToDictionary(plan =>
        plan.EnergySupplier.ToString(), plan =>
        calculateCost(electricityReadings, plan));
    }
```
Now load the price plans from PricePlan store

**3.6 Add more validations for the GetReading method.**
```csharp
    [HttpGet("read/{smartMeterId}")]
    public ObjectResult GetReading(string smartMeterId)
    {
        if (string.IsNullOrEmpty(smartMeterId)) //add more validation to
    existing method
        {
            return new BadRequestObjectResult("Smart meter ID cannot be null or
    empty");
        }
        return new
    OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
    }
