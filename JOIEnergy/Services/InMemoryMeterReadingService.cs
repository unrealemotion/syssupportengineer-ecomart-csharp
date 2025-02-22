﻿using JOIEnergy.Domain;
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
                return readings.OrderBy(r => r.Time).ToList();
            }
            return new List<ElectricityReading>();
        }

        public string StoreReadings(string smartMeterId, List<ElectricityReading> newReadings)
        {
            // 1. Input Validation (separate method)
            string validationError = ValidateNewReadings(newReadings);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // 2. Get Existing Readings (if any)
            if (!_meterReadings.TryGetValue(smartMeterId, out List<ElectricityReading> existingReadings))
            {
                existingReadings = new List<ElectricityReading>();
                _meterReadings[smartMeterId] = existingReadings;
            }

            // 3. Combine and Sort (create a DEEP COPY)
            List<ElectricityReading> combinedReadings = CombineReadings(existingReadings, newReadings);


            // 4.  Validate Combined Readings
            validationError = ValidateCombinedReadings(combinedReadings);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // 5. Store (if all validations pass)
            _meterReadings[smartMeterId] = combinedReadings;


            return CheckForUpdates(existingReadings, newReadings) ? "Reading(s) updated." : string.Empty;
        }

        private string ValidateNewReadings(List<ElectricityReading> newReadings)
        {
            if (newReadings == null || !newReadings.Any())
            {
                return "Cannot store an empty list of readings.";
            }

            if (newReadings.Any(r => r.Time == default))
            {
                return "Cannot store a reading with a default Time value.";
            }
            //check duplicate in new readings
            if (newReadings.GroupBy(r => r.Time).Any(g => g.Count() > 1))
            {
                return "Input readings contain duplicate timestamps.";
            }


            return string.Empty;
        }
        //Validate the combined readings
        private string ValidateCombinedReadings(List<ElectricityReading> combinedReadings)
        {

            for (int i = 0; i < combinedReadings.Count - 1; i++)
            {
                if (combinedReadings[i].Reading > combinedReadings[i + 1].Reading)
                {
                    return $"Readings are not monotonically non-decreasing (time: {combinedReadings[i].Time}).";
                }
            }
            return string.Empty;
        }
        //check for update
        private bool CheckForUpdates(List<ElectricityReading> existingReadings, List<ElectricityReading> newReadings)
        {
            if (existingReadings == null || existingReadings.Count == 0) return false;

            // Check if any new readings have the same timestamp as an existing reading
            return newReadings.Any(newReading => existingReadings.Any(existingReading => existingReading.Time == newReading.Time));
        }

        //Combine, update and sort.
        private List<ElectricityReading> CombineReadings(List<ElectricityReading> existingReadings, List<ElectricityReading> newReadings)
        {
            // Create a deep copy to avoid modifying the original list or its elements.
            List<ElectricityReading> combined = existingReadings.Select(r => new ElectricityReading { Time = r.Time, Reading = r.Reading }).ToList();

            foreach (var newReading in newReadings)
            {
                // Find the reading (if existed) in the *copied* list.
                var existingReading = combined.FirstOrDefault(x => x.Time == newReading.Time);

                if (existingReading != null)
                {
                    // Update the *copy*, not the original.
                    existingReading.Reading = newReading.Reading;
                }
                else
                {
                    // Add a *copy* of the new reading.
                    combined.Add(new ElectricityReading { Time = newReading.Time, Reading = newReading.Reading });
                }
            }
            return combined.OrderBy(r => r.Time).ToList();
        }
    }
}
