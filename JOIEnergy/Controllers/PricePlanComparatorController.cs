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
