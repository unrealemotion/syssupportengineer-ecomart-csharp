using System;
using System.Collections.Generic;
using JOIEnergy.Enums;

namespace JOIEnergy.Services
{
    // Services/AccountService.cs
    // This service manages the association between smart meters and their assigned energy suppliers (price plans).
    // Implements IAccountService and inherits from Dictionary for convenience
    public class AccountService : Dictionary<string, Supplier>, IAccountService
    {
        private Dictionary<string, Supplier> _smartMeterToPricePlanAccounts;

        // Constructor: Initializes the service with a dictionary of smart meter to price plan mappings.
        // Parameters:
        //   smartMeterToPricePlanAccounts: A dictionary where the key is the smart meter ID (string)
        //                                   and the value is the corresponding Supplier enum.
        public AccountService(Dictionary<string, Supplier> smartMeterToPricePlanAccounts)
        {
            _smartMeterToPricePlanAccounts = smartMeterToPricePlanAccounts;
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
    }
}
