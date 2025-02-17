using System.Collections.Generic;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{

    // Services/IMeterReadingService.cs
    // Defines the interface for the MeterReadingService, specifying contracts for storing and retrieving readings.
    public interface IMeterReadingService
    {

        // Retrieves the electricity readings for a given smart meter.
        // Parameters:
        //   smartMeterId: The ID of the smart meter.
        // Returns: A list of ElectricityReading objects.
        List<ElectricityReading> GetReadings(string smartMeterId);

        // Stores electricity readings for a given smart meter.
        // Parameters:
        //   smartMeterId: The ID of the smart meter.
        //   electricityReadings: A list of ElectricityReading objects to store.
        void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings);
    }
}