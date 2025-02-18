using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOIEnergy.Domain;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;

namespace JOIEnergy.Controllers
{
    [Route("readings")]
    public class MeterReadingController : Controller
    {
        private readonly IMeterReadingService _meterReadingService;

        public MeterReadingController(IMeterReadingService meterReadingService)
        {
            _meterReadingService = meterReadingService;
        }

        [HttpPost("store")]
        public ObjectResult Post([FromBody] MeterReadings meterReadings)
        {
            //Comprehensive null validation is still needed for this endpoint.
            if (meterReadings == null)
            {
                return new BadRequestObjectResult("Invalid request - null");
            }
            //moved down
            if (!IsMeterReadingsValid(meterReadings))
            {
                return new BadRequestObjectResult("Internal Server Error");
            }

            if (meterReadings.ElectricityReadings == null)
            {
                return new BadRequestObjectResult("Invalid request - null");
            }
            foreach (var reading in meterReadings.ElectricityReadings)
            {
                if (reading == null)
                {
                    return new BadRequestObjectResult("Invalid request - null");
                }
            }

            string result = _meterReadingService.StoreReadings(meterReadings.SmartMeterId, meterReadings.ElectricityReadings);
            if (string.IsNullOrEmpty(result))
            {
                return new OkObjectResult("Readings stored successfully."); // Success - new readings added
            }
            else if (result == "Reading(s) updated.")
            {
                return new OkObjectResult(result); // Success - readings updated.
            }
            else
            {
                return new BadRequestObjectResult(result); // Return the validation error message
            }

        }
        //Check for null smartMeterId, and empty smartMeterId, and null electricityReadings, and empty electricity readings
        private bool IsMeterReadingsValid(MeterReadings meterReadings)
        {
            // FIXED: Check if meterReadings is null *first*.
            if (meterReadings == null)
            {
                return false;
            }

            // Now it's safe to access meterReadings members.
            string smartMeterId = meterReadings.SmartMeterId;
            List<ElectricityReading> electricityReadings = meterReadings.ElectricityReadings;

            return !string.IsNullOrEmpty(smartMeterId) //better check
                && electricityReadings != null && electricityReadings.Any();
        }

        [HttpGet("read/{smartMeterId}")]
        public ObjectResult GetReading(string smartMeterId)
        {
            if (string.IsNullOrEmpty(smartMeterId)) //add more validation to existing method
            {
                return new BadRequestObjectResult("Smart meter ID cannot be null or empty");
            }
            return new OkObjectResult(_meterReadingService.GetReadings(smartMeterId));
        }
    }
}