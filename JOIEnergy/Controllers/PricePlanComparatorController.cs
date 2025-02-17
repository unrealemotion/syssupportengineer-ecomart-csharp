using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;

//Refactored
namespace JOIEnergy.Controllers
{
    [Route("price-plans")]
    public class PricePlanComparatorController : Controller
    {
        private readonly IPricePlanService _pricePlanService;
        private readonly IAccountService _accountService;
        private readonly IMeterReadingService _meterReadingService;

        // Use constructor injection for dependencies
        public PricePlanComparatorController(IPricePlanService pricePlanService, IAccountService accountService, IMeterReadingService meterReadingService)
        {
            _pricePlanService = pricePlanService;
            _accountService = accountService;
            _meterReadingService = meterReadingService; // Inject MeterReadingService
        }


        [HttpGet("compare-all/{smartMeterId}")]
        public ActionResult<Dictionary<string, decimal>> CalculatedCostForEachPricePlan(string smartMeterId)
        {
            if (string.IsNullOrEmpty(smartMeterId))
            {
                return BadRequest("SmartMeterId cannot be null or empty.");
            }

            Supplier pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
            if (pricePlanId == Supplier.NullSupplier)
            {
                return NotFound($"Smart Meter ID ({smartMeterId}) not found");
            }

            Dictionary<string, decimal> costPerPricePlan = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);
            return costPerPricePlan;
        }

        [HttpGet("recommend/{smartMeterId}")]
        public ActionResult<List<KeyValuePair<string, decimal>>> RecommendCheapestPricePlans(string smartMeterId, int? limit = null)
        {
            if (string.IsNullOrEmpty(smartMeterId))
            {
                return BadRequest("SmartMeterId cannot be null or empty.");
            }
            if (_accountService.GetPricePlanIdForSmartMeterId(smartMeterId) == Supplier.NullSupplier)
            {
                return NotFound($"Smart Meter ID ({smartMeterId}) not found");
            }

            var consumptionForPricePlans = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

            var recommendations = consumptionForPricePlans.OrderBy(pricePlanComparison => pricePlanComparison.Value).ToList();  // Convert to list to support limiting.

            if (limit.HasValue)
            {
                recommendations = recommendations.Take(limit.Value).ToList();
            }

            return recommendations;
        }
    }
}

//using System.Collections.Generic;
//using System.Linq;
//using JOIEnergy.Enums;
//using JOIEnergy.Services;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json.Linq;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace JOIEnergy.Controllers
//{

//    // Controllers/PricePlanComparatorController.cs
//    // This controller handles API requests related to comparing price plans and recommending the best option.
//    [Route("price-plans")]
//    public class PricePlanComparatorController : Controller
//    {
//        private readonly IPricePlanService _pricePlanService;
//        private readonly IAccountService _accountService;

//        // Constructor: Initializes the controller with a price plan service and an account service.
//        // Parameters:
//        //   pricePlanService: An instance of IPricePlanService to handle price plan calculations.
//        //   accountService: An instance of IAccountService to retrieve account-related information.
//        public PricePlanComparatorController(IPricePlanService pricePlanService, IAccountService accountService)
//        {
//            this._pricePlanService = pricePlanService;
//            this._accountService = accountService;
//        }

//        [HttpGet("compare-all/{smartMeterId}")]
//        // Handles GET requests to compare the cost of all price plans for a given smart meter.
//        // Parameters:
//        //   smartMeterId: The ID of the smart meter (string) passed in the URL.
//        // Returns:
//        //   An ObjectResult containing a dictionary of price plan names and their calculated costs.
//        //   Returns 404 Not Found if the smart meter ID is not found.
//        public ObjectResult CalculatedCostForEachPricePlan(string smartMeterId)
//        {
//            Supplier pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
//            Dictionary<string, decimal> costPerPricePlan = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);
//            if (!costPerPricePlan.Any())
//            {
//                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
//            }

//            return
//                costPerPricePlan.Any() ?
//                new ObjectResult(costPerPricePlan) :
//                new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
//        }

//        [HttpGet("recommend/{smartMeterId}")]
//        // Handles GET requests to recommend the cheapest price plans for a given smart meter.
//        // Parameters:
//        //   smartMeterId: The ID of the smart meter (string) passed in the URL.
//        //   limit: An optional integer specifying the maximum number of recommendations to return.
//        // Returns:
//        //   An ObjectResult containing an ordered list of key-value pairs (price plan name and cost).
//        //   Returns 404 Not Found if the smart meter ID is not found.
//        public ObjectResult RecommendCheapestPricePlans(string smartMeterId, int? limit = null)
//        {
//            var consumptionForPricePlans = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

//            if (!consumptionForPricePlans.Any())
//            {
//                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
//            }

//            var recommendations = consumptionForPricePlans.OrderBy(pricePlanComparison => pricePlanComparison.Value);

//            if (limit.HasValue && limit.Value < recommendations.Count())
//            {
//                return new ObjectResult(recommendations.Take(limit.Value));
//            }

//            return new ObjectResult(recommendations);
//        }
//    }
//}
