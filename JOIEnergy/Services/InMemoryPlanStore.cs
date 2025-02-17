using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Generator;
using System;
using System.Collections.Generic;

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
        private List<PeakTimeMultiplier> NoMultipliers()
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

        public Dictionary<string, List<ElectricityReading>> GenerateMeterElectricityReadings()
        {
            var readings = new Dictionary<string, List<ElectricityReading>>();
            var generator = new ElectricityReadingGenerator();
            var smartMeterIds = GetSmartMeterToPricePlanAccounts().Keys;
            foreach (var smartMeterId in smartMeterIds)
            {
                readings.Add(smartMeterId, generator.Generate(20));
            }
            return readings;
        }

    }
}