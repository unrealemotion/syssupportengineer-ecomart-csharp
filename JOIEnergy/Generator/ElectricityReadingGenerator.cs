using System;
using System.Collections.Generic;
using JOIEnergy.Domain;

namespace JOIEnergy.Generator
{

    // Generator/ElectricityReadingGenerator.cs
    // This class generates sample electricity readings for testing or simulation purposes.
    public class ElectricityReadingGenerator
    {

        // Constructor (empty in this case).
        public ElectricityReadingGenerator()
        {

        }
        public List<ElectricityReading> Generate(int number)
        {
            var readings = new List<ElectricityReading>();
            var random = new Random();
            decimal previousReading = 0; // Keep track of the last reading value.

            for (int i = 0; i < number; i++)
            {
                // Generate a reading that's *larger* than the previous one.
                // Add a random increment to the previous reading.  The increment is between 0.0 and 1.0.
                decimal readingIncrement = (decimal)random.NextDouble();
                decimal currentReading = previousReading + readingIncrement;


                var electricityReading = new ElectricityReading
                {
                    Reading = currentReading,
                    Time = DateTime.Now.AddSeconds(i * 10) // Creates readings with timestamps spaced 10 seconds apart.
                };
                readings.Add(electricityReading);
                previousReading = currentReading; // Update previousReading for the next iteration.
            }

            //readings.Sort((reading1, reading2) => reading1.Time.CompareTo(reading2.Time)); // Sorts readings chronologically. No longer required
            return readings;
        }
    }
}
