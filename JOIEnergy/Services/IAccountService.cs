using JOIEnergy.Enums;
using System.Collections.Generic;

namespace JOIEnergy.Services
{

    // Services/IAccountService.cs
    // Defines the interface for the AccountService, specifying the contract for retrieving price plan IDs.
    public interface IAccountService
    {

        // Retrieves the price plan (Supplier) associated with a given smart meter ID.
        // Parameters:
        //   smartMeterId: The ID of the smart meter.
        // Returns: The Supplier enum representing the associated price plan.
        Supplier GetPricePlanIdForSmartMeterId(string smartMeterId);

       
        IEnumerable<string> GetSmartMeterIds(); // Added to support loading readings
    }
}