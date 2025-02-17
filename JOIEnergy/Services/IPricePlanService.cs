using System.Collections.Generic;

namespace JOIEnergy.Services
{

    // Services/IPricePlanService.cs
    // Defines the interface for the PricePlanService, specifying the contract for calculating energy costs.
    public interface IPricePlanService
    {

        // Calculates the consumption cost of electricity readings for each price plan.
        // Parameters:
        //   smartMeterId: The ID of the smart meter.
        // Returns: A dictionary where the key is the price plan name (string) and the value is the calculated cost (decimal).
        Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId);
    }
}