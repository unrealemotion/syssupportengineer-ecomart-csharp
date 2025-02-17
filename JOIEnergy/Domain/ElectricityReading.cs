using Newtonsoft.Json;
using System;
namespace JOIEnergy.Domain
{

    // Domain/ElectricityReading.cs
    // Represents a single electricity reading from a smart meter.
    public class ElectricityReading
    {
        [JsonProperty(PropertyName = "Time")]
        public DateTime Time { get; set; }
        [JsonProperty(PropertyName = "Reading")]
        public Decimal Reading { get; set; }
    }
}
