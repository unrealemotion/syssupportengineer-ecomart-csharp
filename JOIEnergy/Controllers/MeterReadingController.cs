using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Domain;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JOIEnergy.Controllers
{

    // Controllers/MeterReadingController.cs
    // This controller handles API requests related to storing and retrieving meter readings.
    [Route("readings")] // Defines the base route for this controller.
    public class MeterReadingController : Controller
    {
        private readonly IMeterReadingService _meterReadingService;

        // Constructor: Initializes the controller with a meter reading service.
        // Parameters:
        //   meterReadingService: An instance of IMeterReadingService to handle meter reading operations.
        public MeterReadingController(IMeterReadingService meterReadingService)
        {
            _meterReadingService = meterReadingService;
        }

        [HttpPost("store")]
        // Handles POST requests to store meter readings.
        // Parameters:
        //   meterReadings: A MeterReadings object received in the request body, containing the smart meter ID and readings.
        // Returns:
        //   An ObjectResult representing the outcome of the operation.
        //   Returns 200 OK with an empty body on success.
        //   Returns 400 Bad Request if the input is invalid.
        public ObjectResult Post([FromBody] MeterReadings meterReadings)
        {
            if (!IsMeterReadingsValid(meterReadings))
            {
                return new BadRequestObjectResult("Internal Server Error");
            }
            _meterReadingService.StoreReadings(meterReadings.SmartMeterId, meterReadings.ElectricityReadings);
            return new OkObjectResult("{}");
        }

        // Validates the MeterReadings object received in the request.
        // Parameters:
        //  meterReadings: the MeterReading object
        // Returns:
        //   True if the MeterReadings is valid, false otherwise.
        private bool IsMeterReadingsValid(MeterReadings meterReadings)
        {
            // Check if meterReadings itself is null
            if (meterReadings == null)
            {
                return false;
            }

            // Directly check properties of meterReadings for null and emptiness
            return meterReadings.SmartMeterId != null
                   && meterReadings.SmartMeterId.Any()
                   && meterReadings.ElectricityReadings != null
                   && meterReadings.ElectricityReadings.Any();
        }
        //{
        //    String smartMeterId = meterReadings.SmartMeterId;
        //    List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;
        //    // FIXED: Corrected logic to check for non-null and non-empty.
        //    return smartMeterId != null && smartMeterId.Any()
        //        && electricityReadings != null && electricityReadings.Any();
        //}

        [HttpGet("read/{smartMeterId}")]
        // Handles GET requests to retrieve meter readings for a specific smart meter.
        // Parameters:
        //   smartMeterId: The ID of the smart meter (string) passed in the URL.
        // Returns:
        //   An ObjectResult containing the list of ElectricityReading objects for the specified smart meter.
        public ObjectResult GetReading(string smartMeterId)
        {
            return new OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
        }
    }
}
