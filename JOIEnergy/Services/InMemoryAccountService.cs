using JOIEnergy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JOIEnergy.Services
{
    public class InMemoryAccountService : IAccountService
    {
        private readonly Dictionary<string, Supplier> _smartMeterToPricePlanAccounts;

        public InMemoryAccountService(IPricePlanStore pricePlanStore)
        {
            _smartMeterToPricePlanAccounts = pricePlanStore.GetSmartMeterToPricePlanAccounts();
        }

        public Supplier GetPricePlanIdForSmartMeterId(string smartMeterId)
        {
            if (_smartMeterToPricePlanAccounts.TryGetValue(smartMeterId, out var supplier))
            {
                return supplier;
            }

            return Supplier.NullSupplier;
        }
        public IEnumerable<string> GetSmartMeterIds()
        {
            return _smartMeterToPricePlanAccounts.Keys.ToList();
        }
    }
}