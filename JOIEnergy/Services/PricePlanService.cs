using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;
using System;
namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        private readonly IMeterReadingService _meterReadingService;
        private readonly IPricePlanStore _pricePlanStore;
        private readonly IDateTimeProvider _dateTimeProvider;
        public PricePlanService(IMeterReadingService meterReadingService, IPricePlanStore pricePlanStore, IDateTimeProvider dateTimeProvider)
        {
            _meterReadingService = meterReadingService;
            _pricePlanStore = pricePlanStore;
            _dateTimeProvider = dateTimeProvider;
        }
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            if (pricePlan == null) throw new ArgumentNullException("Price plan cannot be null.", nameof(pricePlan));
            decimal totalCost = 0;
            // Iterate through consecutive pairs of readings.
            //sort by time
            electricityReadings = electricityReadings.OrderBy(x => x.Time).ToList();
            for (int i = 0; i < electricityReadings.Count - 1; i++)
            {
                // Calculate the energy consumed during this interval.
                decimal energyConsumed = electricityReadings[i + 1].Reading - electricityReadings[i].Reading;
                if (energyConsumed < 0)
                {
                    throw new ArgumentException($"Meter reading {i + 1} cannot have reading less than the previous one, please check your data", nameof(electricityReadings));
                }
                // Use the GetPrice method of PricePlan to get correct price based on the reading time
                totalCost += pricePlan.GetPrice(electricityReadings[i].Time) * energyConsumed;
            }
            return totalCost;
        }

        public Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId)
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
