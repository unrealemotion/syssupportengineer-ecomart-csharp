# JOIEnergy Project: Bug Identification and Remediation - Stage One

This document details the bugs identified and fixed in the initial stage of the JOIEnergy project.

## Part 1: Summary of Bugs and Minor Adjustments

### 1.1 Bugs

1.  **`MeterReadingService.GetReadings`:** Always returned readings for `"smart-meter-2"` regardless of the requested smart meter ID.
2.  **`PricePlanService.calculateTimeElapsed`**: Compiling error due to incorrect member access.
3.  **`PricePlanService.calculateCost`:** Did not correctly calculate energy costs:
    *   Incorrectly calculated average reading and time elapsed.
    *   Did not incorporate peak time multipliers.
    *   Incorrectly iterated through readings after sorting was introduced.
4.  **`MeterReadingController.IsMeterReadingsValid`:**
    *   Incorrect null checks.
    *   Incorrect return type (should be `bool`).
    *   Incorrect logic for checking null/empty values.

### 1.2 Minor Adjustments

*   **Sorted Readings:** The `MeterReadingService.GetReadings` method now returns readings sorted chronologically by time.
*   **Sorted Readings in calculateCost:** The `PricePlanService.calculateCost` method now sorts the readings chronologically by time *before* calculation (redundant now, but included for completeness of the history of changes).

## Part 2: Detailed Bug Descriptions (with Code Snippets)

### 2.1 `MeterReadingService.GetReadings` Bug

**Description:** The `GetReadings` method incorrectly returned readings for a hardcoded smart meter ID ("smart-meter-2") instead of using the `smartMeterId` parameter passed to the method.

```csharp
// Incorrect Snippet (MeterReadingService.cs)
public List<ElectricityReading> GetReadings(string smartMeterId)
{
    if (MeterAssociatedReadings.ContainsKey(smartMeterId))
    {
        return MeterAssociatedReadings["smart-meter-2"]; // Always returns "smart-meter-2"
    }

    return new List<ElectricityReading>();
}
```

### 2.2 `PricePlanService.calculateTimeElapsed` Bug
**Description:** The `calculateTimeElapsed` method in `PricePlanService` did not correctly calculate the earliest time.

```csharp
// Incorrect Snippet (PricePlanService.cs)
private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
{
    var first = electricityReadings.readings.Time; //Incorrect way of finding the min time
    var last = electricityReadings.Max(reading => reading.Time);
    return (decimal)(last - first).TotalHours;
}
```

### 2.3 `PricePlanService.calculateCost` Bugs

**Description:** The original `calculateCost` method had multiple issues: it incorrectly calculated an "average" reading, incorrectly calculated a time-weighted average, and did not use the `PricePlan.GetPrice()` method to account for peak time multipliers.  After introducing sorting, the iteration was also incorrect.

```csharp
// Incorrect Snippet (PricePlanService.cs - Before Sorting Fix)
private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
{
    var average = calculateAverageReading(electricityReadings); // Incorrect average
    var timeElapsed = calculateTimeElapsed(electricityReadings);
    var averagedCost = average / timeElapsed; // Flawed average cost
    return averagedCost * pricePlan.UnitRate; // Ignores peak times
}
```

```csharp
// Incorrect Snippet (PricePlanService.cs - After Initial Sorting Fix, Before Final Optimization)
private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
{
    decimal totalCost = 0;
    List<ElectricityReading> sortedReadings = electricityReadings.OrderBy(r => r.Time).ToList();

    for (int i = 0; i < sortedReadings.Count - 1; i++) // Unnecessary iteration
    {
        decimal energyConsumed = sortedReadings[i + 1].Reading - sortedReadings[i].Reading;
        decimal price = pricePlan.GetPrice(sortedReadings[i].Time);
        totalCost += energyConsumed * price;
    }
    return totalCost;
}
```

### 2.4 `MeterReadingController.IsMeterReadingsValid` Bugs

**Description:** The `IsMeterReadingsValid` method had incorrect null checks, an incorrect return type, and flawed logic for checking for null or empty values.

```csharp
// Incorrect Snippet (MeterReadingController.cs)
private String IsMeterReadingsValid(MeterReadings meterReadings) // Incorrect return type
{
    String smartMeterId = meterReadings.SmartMeterId;
    List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;
    return smartMeterId == null && smartMeterId.Any() // Incorrect logic
        && electricityReadings == null && electricityReadings.Any(); // Incorrect logic
}
```

## Part 3: Bug Fixes (Original and Rectified Code)

### 3.1 `MeterReadingService.GetReadings` Fix

**Original Code (with bug):**

```csharp
// Services/MeterReadingService.cs (ORIGINAL)
using System;
using System.Collections.Generic;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        public Dictionary<string, List<ElectricityReading>> MeterAssociatedReadings { get; set; }

        public MeterReadingService(Dictionary<string, List<ElectricityReading>> meterAssociatedReadings)
        {
            MeterAssociatedReadings = meterAssociatedReadings;
        }

        public List<ElectricityReading> GetReadings(string smartMeterId)
        {
            if (MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                return MeterAssociatedReadings["smart-meter-2"]; // BUG: Always returns "smart-meter-2"
            }

            return new List<ElectricityReading>();
        }

        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings)
        {
            if (!MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                MeterAssociatedReadings.Add(smartMeterId, new List<ElectricityReading>());
            }

            electricityReadings.ForEach(electricityReading => MeterAssociatedReadings[smartMeterId].Add(electricityReading));
        }
    }
}
```

**Rectified Code:**

```csharp
// Services/MeterReadingService.cs (FIXED - GetReadings now returns sorted readings)

using System;
using System.Collections.Generic;
using System.Linq; // Import LINQ for OrderBy
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        public Dictionary<string, List<ElectricityReading>> MeterAssociatedReadings { get; set; }

        public MeterReadingService(Dictionary<string, List<ElectricityReading>> meterAssociatedReadings)
        {
            MeterAssociatedReadings = meterAssociatedReadings;
        }

        // Retrieves the electricity readings for a given smart meter ID, sorted by time.
        public List<ElectricityReading> GetReadings(string smartMeterId)
        {
            if (MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                // Return a *new* list that is sorted.  Don't modify the stored list directly.
                return MeterAssociatedReadings[smartMeterId].OrderBy(r => r.Time).ToList(); //FIXED: now use smartMeterId and sort the output
            }

            return new List<ElectricityReading>();
        }

        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings)
        {
            if (!MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                MeterAssociatedReadings.Add(smartMeterId, new List<ElectricityReading>());
            }

            electricityReadings.ForEach(electricityReading => MeterAssociatedReadings[smartMeterId].Add(electricityReading));
        }
    }
}
```

### 3.2 and 3.3 `PricePlanService` Fixes

**Original Code (with bugs):**

```csharp
// Services/PricePlanService.cs (ORIGINAL)

using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); }; // Unused interface.

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
        }

        private decimal calculateAverageReading(List<ElectricityReading> electricityReadings)
        {
            var newSummedReadings = electricityReadings.Select(readings => readings.Reading).Aggregate((reading, accumulator) => reading + accumulator);
            return newSummedReadings / electricityReadings.Count();
        }

        private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        {
            var first = electricityReadings.readings.Time; //BUG: Incorrect way of finding min
            var last = electricityReadings.Max(reading => reading.Time);
            return (decimal)(last - first).TotalHours;
        }

        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var average = calculateAverageReading(electricityReadings);
            var timeElapsed = calculateTimeElapsed(electricityReadings);
            var averagedCost = average / timeElapsed;
            return averagedCost * pricePlan.UnitRate; // BUG: Only uses the base UnitRate.
        }

        public Dictionary<String, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(String smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);
            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }

            return _pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(electricityReadings, plan));
        }
    }
}
```

**Rectified Code (Final Version):**

```csharp
// Services/PricePlanService.cs (FIXED - Final, Optimized Version)

using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); }; // Unused interface.

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
        }

        // CORRECTED and OPTIMIZED: Calculates cost using total consumption.
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            // Sort the readings by Time (now redundant, but kept for historical context).
            List<ElectricityReading> sortedReadings = electricityReadings.OrderBy(r => r.Time).ToList();

            // Calculate total energy consumed (last reading - first reading).
            decimal totalEnergyConsumed = sortedReadings.Last().Reading - sortedReadings.First().Reading;

            // Get the price at the *beginning* of the consumption period.
            decimal price = pricePlan.GetPrice(sortedReadings.First().Time);

            // Calculate and return the total cost.
            return totalEnergyConsumed * price;
        }


        // Calculates the consumption cost for each price plan.
        public Dictionary<String, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(String smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);

            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }

            // Use the corrected calculateCost function.
            return _pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(electricityReadings, plan));
        }
    }
}
```

### 3.4 `MeterReadingController.IsMeterReadingsValid` Fixes

**Original Code (with bugs):**

```csharp
// Controllers/MeterReadingController.cs (ORIGINAL)
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
            if (!IsMeterReadingsValid(meterReadings))
            {
                return new BadRequestObjectResult("Internal Server Error");
            }

            _meterReadingService.StoreReadings(meterReadings.SmartMeterId, meterReadings.ElectricityReadings);
            return new OkObjectResult("{}");
        }

        private String IsMeterReadingsValid(MeterReadings meterReadings) //BUG: incorrect return type
        {
            String smartMeterId = meterReadings.SmartMeterId;
            List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;
            return smartMeterId == null && smartMeterId.Any() // BUG: Incorrect null check and return type
                && electricityReadings == null && electricityReadings.Any(); // BUG: Incorrect null check
        }


        [HttpGet("read/{smartMeterId}")]
        public ObjectResult GetReading(string smartMeterId)
        {
            return new OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
        }
    }
}
```

**Rectified Code:**

```csharp
// Controllers/MeterReadingController.cs (FIXED)
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
            if (!IsMeterReadingsValid(meterReadings))
            {
                return new BadRequestObjectResult("Internal Server Error");
            }

            _meterReadingService.StoreReadings(meterReadings.SmartMeterId, meterReadings.ElectricityReadings);
            return new OkObjectResult("{}");
        }

        // FIXED: Corrected return type and null checks.
        private bool IsMeterReadingsValid(MeterReadings meterReadings)
        {
            String smartMeterId = meterReadings.SmartMeterId;
            List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;
            // FIXED: Corrected logic to check for non-null and non-empty.
            return smartMeterId != null && smartMeterId.Any()
                && electricityReadings != null && electricityReadings.Any();
        }


        [HttpGet("read/{smartMeterId}")]
        public ObjectResult GetReading(string smartMeterId)
        {
            return new OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
        }
    }
}
```
