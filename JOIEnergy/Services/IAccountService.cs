using JOIEnergy.Enums;

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
    }
}