using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Enums;

namespace JOIEnergy.Domain
{

    // Represents an energy price plan offered by a supplier.
    public class PricePlan
    {

        // Gets or sets the energy supplier.
        public Supplier EnergySupplier { get; set; }

        // Gets or sets the base unit rate (e.g., price per kWh).
        public decimal UnitRate { get; set; }

        // Gets or sets the list of peak time multipliers.
        public IList<PeakTimeMultiplier> PeakTimeMultiplier { get; set; }

        // Get price by datetime
        // Parameters:
        //   datetime: the datetime to check
        // Returns: price
        public decimal GetPrice(DateTime datetime)
        {
            var multiplier = PeakTimeMultiplier.FirstOrDefault(m => m.DayOfWeek == datetime.DayOfWeek);

            if (multiplier?.Multiplier != null)
            {
                return multiplier.Multiplier * UnitRate;
            }
            else
            {
                return UnitRate;
            }
        }
    }

    // Nested class representing a peak time multiplier.
    public class PeakTimeMultiplier
    {
        public DayOfWeek DayOfWeek { get; set; }
        public decimal Multiplier { get; set; }
    }
}
