using JOIEnergy.Domain;
using JOIEnergy.Enums;
using System.Collections.Generic;

namespace JOIEnergy.Services
{
    public interface IPricePlanStore
    {
        List<PricePlan> GetPricePlans();

        Dictionary<string, Supplier> GetSmartMeterToPricePlanAccounts();
    }
}