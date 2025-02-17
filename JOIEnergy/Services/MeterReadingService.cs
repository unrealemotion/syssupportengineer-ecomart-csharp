using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        public Dictionary<string, List<ElectricityReading>> MeterAssociatedReadings { get; set; }

        public MeterReadingService(Dictionary<string, List<ElectricityReading>> meterAssociatedReadings)
        {
            MeterAssociatedReadings = meterAssociatedReadings;
        }

        // Retrieves the electricity readings for a given smart meter ID.
        public List<ElectricityReading> GetReadings(string smartMeterId)
        {
            // FIXED: Use the provided smartMeterId parameter.
            if (MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                // Return a *new* list that is sorted.  Don't modify the stored list directly.
                return MeterAssociatedReadings[smartMeterId].OrderBy(r => r.Time).ToList();
            }

            return new List<ElectricityReading>();
        }

        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings)
        {
            if (!MeterAssociatedReadings.ContainsKey(smartMeterId))
            {
                MeterAssociatedReadings.Add(smartMeterId, new List<ElectricityReading>());
            }

            electricityReadings.ForEach(electricityReading => MeterAssociatedReadings[smartMeterId].Add(electricityReading));
        }
    }
}

//{

//    // Services/MeterReadingService.cs
//    // This service manages the storage and retrieval of electricity readings.  It uses an in-memory dictionary
//    // for storage, which means data is NOT persistent across application restarts.
//    public class MeterReadingService : IMeterReadingService
//    {

//        // In-memory storage for meter readings.  Key is the smart meter ID, value is a list of readings.
//        public Dictionary<string, List<ElectricityReading>> MeterAssociatedReadings { get; set; }

//        // Constructor: Initializes the service with a dictionary of meter readings.
//        // Parameters:
//        //   meterAssociatedReadings: A dictionary where the key is the smart meter ID (string)
//        //                           and the value is a list of ElectricityReading objects.
//        public MeterReadingService(Dictionary<string, List<ElectricityReading>> meterAssociatedReadings)
//        {
//            MeterAssociatedReadings = meterAssociatedReadings;
//        }

//        // Retrieves the electricity readings for a given smart meter ID.
//        // Parameters:
//        //   smartMeterId: The ID of the smart meter (string).
//        // Returns:
//        //   If the smartMeterID is valid, it will always return a list of "smart-meter-2" reading.
//        //   It should, however, return a list of ElectricityReading objects for the specified smart meter.
//        //   Returns an empty list if no readings are found for the given ID.
//        public List<ElectricityReading> GetReadings(string smartMeterId) {
//            if (MeterAssociatedReadings.ContainsKey(smartMeterId)) {
//                return MeterAssociatedReadings["smart-meter-2"];
//            }
//            return new List<ElectricityReading>();
//        }

//        // Stores electricity readings for a given smart meter ID.
//        // Parameters:
//        //   smartMeterId: The ID of the smart meter (string).
//        //   electricityReadings: A list of ElectricityReading objects to store.
//        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings) {
//            if (!MeterAssociatedReadings.ContainsKey(smartMeterId)) {
//                MeterAssociatedReadings.Add(smartMeterId, new List<ElectricityReading>());
//            }

//            electricityReadings.ForEach(electricityReading => MeterAssociatedReadings[smartMeterId].Add(electricityReading));
//        }
//    }
//}
