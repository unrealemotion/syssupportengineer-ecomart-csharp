using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{

    // Services/PricePlanService.cs
    // This service calculates the cost of electricity consumption based on meter readings and price plans.
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); };

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;

        // Constructor: Initializes the service with a list of price plans and a meter reading service.
        // Parameters:
        //   pricePlan: A list of PricePlan objects representing the available energy plans.
        //   meterReadingService: An instance of IMeterReadingService to retrieve meter readings.
        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
        }

        //// Calculates the average reading value from a list of electricity readings.
        //// Parameters:
        ////   electricityReadings: A list of ElectricityReading objects.
        //// Returns: The average reading value (decimal).
        //private decimal calculateAverageReading(List<ElectricityReading> electricityReadings)
        //{
        //    var newSummedReadings = electricityReadings.Select(readings => readings.Reading).Aggregate((reading, accumulator) => reading + accumulator);

        //    return newSummedReadings / electricityReadings.Count();
        //}

        //// Calculates the total time elapsed between the first and last readings in a list.
        //// Parameters:
        ////   electricityReadings: A list of ElectricityReading objects.
        //// Returns: The time elapsed in hours (decimal).
        //private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        //{
        //    var first = electricityReadings.Min(reading => reading.Time);
        //    var last = electricityReadings.Max(reading => reading.Time);

        //    return (decimal)(last - first).TotalHours;
        //}

        //// Calculates the cost of electricity consumption for a given list of readings and a specific price plan.
        //// Parameters:
        ////   electricityReadings: A list of ElectricityReading objects.
        ////   pricePlan: The PricePlan object for which to calculate the cost.
        //// Returns: The calculated cost (decimal).  *Does NOT currently consider peak time multipliers.*
        //private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        //{
        //    var average = calculateAverageReading(electricityReadings);
        //    var timeElapsed = calculateTimeElapsed(electricityReadings);
        //    var averagedCost = average/timeElapsed;
        //    return averagedCost * pricePlan.UnitRate;
        //}

        //FIXED
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            decimal totalCost = 0;

            // Sort the readings by Time before processing.  This is crucial!
            List<ElectricityReading> sortedReadings = electricityReadings.OrderBy(r => r.Time).ToList();


            // Iterate through consecutive pairs of readings.
            for (int i = 0; i < sortedReadings.Count - 1; i++)
            {
                // Calculate the energy consumed during this interval.
                decimal energyConsumed = sortedReadings[i + 1].Reading - sortedReadings[i].Reading;

                // Get the correct price for the *start* of the interval, considering peak times.
                decimal price = pricePlan.GetPrice(sortedReadings[i].Time);

                // Calculate the cost for this interval and add it to the total.
                totalCost += energyConsumed * price;
            }

            return totalCost;
        }

        // Calculates the consumption cost of electricity readings for each available price plan.
        // Parameters:
        //   smartMeterId: The ID of the smart meter for which to calculate costs.
        // Returns:
        //   A dictionary where the key is the name of the energy supplier (string)
        //   and the value is the calculated cost (decimal) for that price plan.
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
