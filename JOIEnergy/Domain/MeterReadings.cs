using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JOIEnergy.Domain
{

    // Domain/MeterReadings.cs
    // Represents a collection of electricity readings for a specific smart meter.
    public class MeterReadings
    {
        [JsonProperty(PropertyName = "SmartMeterId")]
        public string SmartMeterId { get; set; }
        [JsonProperty(PropertyName = "ElectricityReadings")]
        public List<ElectricityReading> ElectricityReadings { get; set; }
    }
}
