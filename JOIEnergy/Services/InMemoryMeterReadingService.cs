using JOIEnergy.Domain;
using System.Collections.Generic;
using System;
using System.Linq;

namespace JOIEnergy.Services
{
    public class InMemoryMeterReadingService : IMeterReadingService
    {
        private readonly Dictionary<string, List<ElectricityReading>> _meterReadings = new Dictionary<string, List<ElectricityReading>>();

        public List<ElectricityReading> GetReadings(string smartMeterId)
        {
            if (_meterReadings.TryGetValue(smartMeterId, out var readings))
            {
                return readings.OrderBy(r => r.Time).ToList(); // Ensure readings are ordered.
            }

            return new List<ElectricityReading>(); // Return empty list if no readings found.
        }

        public string StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings)
        {
            if (electricityReadings == null || !electricityReadings.Any())
            {
                return "Cannot store an empty list of readings";
            }

            //validation to ensure Time is set.
            if (electricityReadings.Any(x => x.Time == default))
            {
                return "Cannot store an reading that has time equals to default";
            }

            // Ensure readings are sorted by time
            electricityReadings = electricityReadings.OrderBy(r => r.Time).ToList();

            if (!_meterReadings.ContainsKey(smartMeterId))
            {
                _meterReadings.Add(smartMeterId, new List<ElectricityReading>());
            }

            List<ElectricityReading> existingReadings = _meterReadings[smartMeterId];

            foreach (var newReading in electricityReadings)
            {
                // Check for time uniqueness
                var existingReading = existingReadings.FirstOrDefault(r => r.Time == newReading.Time);
                if (existingReading != null)
                {
                    // Update existing reading
                    if (!IsValidReadingValue(existingReadings, newReading, existingReading)) return $"Invalid value - new reading's value {newReading.Reading} at {newReading.Time} does not fit in with existing readings for {smartMeterId}.";

                    existingReading.Reading = newReading.Reading;
                    return $"Reading updated for time {newReading.Time}"; // Return update message.
                }
                else
                {
                    // Add new reading
                    if (!IsValidReadingValue(existingReadings, newReading, null)) return $"Invalid value - new reading's value {newReading.Reading} at {newReading.Time} does not fit in with existing readings for {smartMeterId}.";

                    existingReadings.Add(newReading);
                }
            }

            return string.Empty; // Return empty string for success
        }

        private bool IsValidReadingValue(List<ElectricityReading> existingReadings, ElectricityReading newReading, ElectricityReading? existingReading)
        {
            //Find index. If existReading is not null, find it.
            //Other wise find the insert position.
            int index = existingReading != null ?
                existingReadings.IndexOf(existingReading) :
                existingReadings.FindIndex(r => r.Time > newReading.Time);

            if (index > 0)
            {
                // Check preceding reading
                if (newReading.Reading < existingReadings[index - 1].Reading)
                {
                    return false;
                }
            }

            if (index < 0)
            {
                index = existingReadings.Count();
            }

            if (index < existingReadings.Count)
            {
                // Check succeeding reading
                if (newReading.Reading > existingReadings[index].Reading)
                {
                    return false;
                }
            }
            return true;

        }
    }
}



//using JOIEnergy.Domain;
//using System.Collections.Generic;
//using System;
//using System.Linq;

//namespace JOIEnergy.Services
//{
//    public class InMemoryMeterReadingService : IMeterReadingService
//    {
//        private readonly Dictionary<string, List<ElectricityReading>> _meterReadings = new Dictionary<string, List<ElectricityReading>>();

//        public List<ElectricityReading> GetReadings(string smartMeterId)
//        {
//            if (_meterReadings.TryGetValue(smartMeterId, out var readings))
//            {
//                return readings.OrderBy(r => r.Time).ToList(); // Ensure readings are ordered.
//            }

//            return new List<ElectricityReading>(); // Return empty list if no readings found.
//        }

//        public void StoreReadings(string smartMeterId, List<ElectricityReading> electricityReadings)
//        {
//            if (electricityReadings == null || !electricityReadings.Any())
//            {
//                throw new ArgumentException("Cannot store an empty list of readings", nameof(electricityReadings));
//            }
//            //validation to ensure Time is set.
//            if (electricityReadings.Any(x => x.Time == default))
//            {
//                throw new ArgumentException("Cannot store an reading that has time equals to default", nameof(electricityReadings));
//            }
//            if (_meterReadings.ContainsKey(smartMeterId))
//            {
//                _meterReadings[smartMeterId].AddRange(electricityReadings);
//            }
//            else
//            {
//                _meterReadings.Add(smartMeterId, electricityReadings);
//            }
//        }
//    }
//}