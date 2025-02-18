# JOIEnergy Project - README

This document provides an overview of the JOIEnergy project, including its purpose, API endpoints, setup instructions, and usage examples.

## Table of Contents

1.  [Project Overview](#project-overview)
    *   [Welcome to PowerDale](#welcome-to-powerdale)
    *   [Introducing JOI Energy](#introducing-joi-energy)
    *   [Problem Statement](#problem-statement)
    *   [Users](#users)
2.  [API Documentation](#api-documentation)
    *   [Store Readings](#store-readings)
    *   [Get Stored Readings](#get-stored-readings)
    *   [View Current Price Plan and Compare Usage Cost](#view-current-price-plan-and-compare-usage-cost)
    *   [View Recommended Price Plans](#view-recommended-price-plans)
3.  [Setup and Usage](#setup-and-usage)
    *   [Requirements](#requirements)
    *   [Compatible IDEs](#compatible-ides)
    *   [Useful Commands](#useful-commands)
        *   [Build the Project](#build-the-project)
        *   [Run the Tests](#run-the-tests)
        *   [Run the Application](#run-the-application)
4. [Code Structure](#code-structure)
5. [Improvements and Fixes](#improvements)

## 1. Project Overview <a name="project-overview"></a>

### Welcome to PowerDale <a name="welcome-to-powerdale"></a>

PowerDale is a fictional small town with approximately 100 residents.  Most houses have smart meters installed, capable of saving and transmitting energy usage data.  Three primary energy providers operate in PowerDale, each with distinct pricing structures:

*   Dr. Evil's Dark Energy
*   The Green Eco
*   Power for Everyone

### Introducing JOI Energy <a name="introducing-joi-energy"></a>

JOI Energy (formerly EcoMart) is an energy industry startup.  Instead of selling energy directly, JOIEnergy differentiates itself by analyzing customer energy consumption data from smart meters and recommending the most cost-effective energy supplier based on individual usage patterns.

### Problem Statement <a name="problem-statement"></a>

The project's immediate goal is to stabilize the existing application by maintaining and improving the code. Simultaneously, the development team is working on a new API for customer and smart meter interaction.  Due to staffing shortages (annual leave and sickness), the support team is under-resourced, making it critical to maintain application stability and minimize disruptions.

### Users <a name="users"></a>

Five members of the JOI accounts team are participating in a trial of the new JOI software, sharing their energy data for testing purposes.  Their details are:

| User    | Smart Meter ID | Power Supplier        |
| :------ | :------------- | :-------------------- |
| Sarah   | smart-meter-0  | Dr. Evil's Dark Energy |
| Peter   | smart-meter-1  | The Green Eco          |
| Charlie | smart-meter-2  | Dr. Evil's Dark Energy |
| Andrea  | smart-meter-3  | Power for Everyone     |
| Alex    | smart-meter-4  | The Green Eco          |

These user details and smart meter IDs are used throughout the application and in the API examples.

## 2. API Documentation <a name="api-documentation"></a>

JOIEnergy provides a RESTful API for managing and analyzing energy consumption data.  All API endpoints are relative to the base URL (typically `http://localhost:5000` when running locally).

### Store Readings <a name="store-readings"></a>

**Endpoint:** `POST /readings/store`

**Description:** Stores electricity readings for a specific smart meter.

**Request Body (JSON):**

```json
{
  "smartMeterId": "<smartMeterId>",
  "electricityReadings": [
    { "time": "<time>", "reading": <reading> },
    { "time": "<time>", "reading": <reading> },
    ...
  ]
}
content_copy
download
Use code with caution.
Markdown

smartMeterId: (string, required) The ID of the smart meter (e.g., "smart-meter-0").

electricityReadings: (array, required) An array of electricity reading objects.

time: (string, required) The timestamp of the reading in ISO 8601 format (e.g., "2020-11-11T08:00:00.0000000+00:00").

reading: (decimal, required) The electricity reading value (delta since the last reading).

Response:

200 OK: Returns an empty JSON object {} on success.

400 Bad Request: Returns "Internal Server Error" if the input is invalid (e.g., missing smartMeterId or electricityReadings, or if the readings are not in chronological order).

Example (using curl):

curl -X POST \
  -H "Content-Type: application/json" \
  "http://localhost:5000/readings/store" \
  -d '{
    "smartMeterId": "smart-meter-0",
    "electricityReadings": [
      {"time": "2020-11-11T08:00:00Z", "reading": 0.0503},
      {"time": "2020-11-11T08:00:10Z", "reading": 0.0505},
      {"time": "2020-11-11T08:00:20Z", "reading": 0.0510},
      {"time": "2020-11-12T08:00:00Z", "reading": 0.0213}
    ]
  }'
content_copy
download
Use code with caution.
Bash
Get Stored Readings <a name="get-stored-readings"></a>

Endpoint: GET /readings/read/{smartMeterId}

Description: Retrieves the stored electricity readings for a specific smart meter.

Parameters:

smartMeterId: (string, required) The ID of the smart meter.

Response:

200 OK: Returns a JSON array of electricity reading objects, sorted by time, in the same format as the electricityReadings array in the POST /readings/store request. Returns an empty array [] if no readings are found for the given smartMeterId.

Example (using curl):

curl "http://localhost:5000/readings/read/smart-meter-0"
content_copy
download
Use code with caution.
Bash

Example Response:

[
  { "time": "2020-11-11T08:00:00Z", "reading": 0.0503 },
  { "time": "2020-11-11T08:00:10Z", "reading": 0.0505 },
  { "time": "2020-11-11T08:00:20Z", "reading": 0.0510 },
  { "time": "2020-11-12T08:00:00Z", "reading": 0.0213 }
]
content_copy
download
Use code with caution.
Json
View Current Price Plan and Compare Usage Cost <a name="view-current-price-plan-and-compare-usage-cost"></a>

Endpoint: GET /price-plans/compare-all/{smartMeterId}

Description: Calculates and returns the total cost of electricity consumption for a given smart meter, for each available price plan.

Parameters:

smartMeterId: (string, required) The ID of the smart meter.

Response:

200 OK: Returns a JSON object where the keys are the names of the energy suppliers (e.g., "DrEvilsDarkEnergy") and the values are the calculated costs (decimal) for that supplier's price plan.

404 Not Found: Returns a string indicating that the smart meter ID was not found. This occurs if the smart meter ID does not exist in the AccountService's _smartMeterToPricePlanAccounts dictionary, or if there are no readings available for the given smart meter.

Example (using curl):

curl "http://localhost:5000/price-plans/compare-all/smart-meter-0"
content_copy
download
Use code with caution.
Bash

Example Response:

{
  "DrEvilsDarkEnergy": 94.87181867550794,
  "TheGreenEco": 18.974363735101587,
  "PowerForEveryone": 9.487181867550794
}
content_copy
download
Use code with caution.
Json
View Recommended Price Plans <a name="view-recommended-price-plans"></a>

Endpoint: GET /price-plans/recommend/{smartMeterId}?limit={limit}

Description: Returns a list of recommended price plans, ordered from cheapest to most expensive, based on the historical consumption data of the specified smart meter. An optional limit parameter can be used to restrict the number of recommendations returned.

Parameters:

smartMeterId: (string, required) The ID of the smart meter.

limit: (integer, optional) The maximum number of price plans to return.

Response:

200 OK: Returns a JSON array of key-value pairs, sorted by cost (ascending). Each key-value pair represents a price plan, with the "key" being the supplier name and the "value" being the calculated cost.

404 Not Found: Returns if readings are not available for that Smart Meter ID

Example (using curl):

curl "http://localhost:5000/price-plans/recommend/smart-meter-0?limit=2"
content_copy
download
Use code with caution.
Bash

Example Response:

[
  {
    "key": "PowerForEveryone",
    "value": 9.487181867550794
  },
  {
    "key": "TheGreenEco",
    "value": 18.974363735101587
  }
]
content_copy
download
Use code with caution.
Json
3. Setup and Usage <a name="setup-and-usage"></a>
Requirements <a name="requirements"></a>

.NET 6.0 SDK (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Compatible IDEs <a name="compatible-ides"></a>

The project has been tested with:

Visual Studio 2022 (17.1)

Visual Studio for Mac (8.10)

Visual Studio Code (1.64) with the C# extension

Useful Commands <a name="useful-commands"></a>

These commands should be executed from the root directory of the project (where the JOIEnergy.csproj file is located).

Build the Project <a name="build-the-project"></a>
dotnet build
content_copy
download
Use code with caution.
Bash

This command compiles the project and its dependencies.

Run the Tests <a name="run-the-tests"></a>
dotnet test JOIEnergy.Tests
content_copy
download
Use code with caution.
Bash

This command runs the unit tests in the JOIEnergy.Tests project.

Run the Application <a name="run-the-application"></a>
dotnet run --project JOIEnergy
content_copy
download
Use code with caution.
Bash

This command starts the ASP.NET Core web application. The application will be accessible at http://localhost:5000 by default.

4. Code Structure <a name="code-structure"></a>

The project is organized into the following main folders:

Controllers: Contains the API controllers (MeterReadingController.cs and PricePlanComparatorController.cs).

Domain: Contains the domain models (ElectricityReading.cs, MeterReadings.cs, PricePlan.cs, and Supplier.cs).

Enums: Supplier Enums (Supplier.cs)

Generator: Contains the ElectricityReadingGenerator.cs class for generating sample data.

Services: Contains the service classes that implement the business logic (AccountService.cs, IMeterReadingService.cs, IPricePlanService.cs, MeterReadingService.cs, and PricePlanService.cs).

JOIEnergy.Tests Contains the Unit Tests.

Key Classes and their responsibilities:

MeterReadingController: Handles requests related to storing and retrieving meter readings.

PricePlanComparatorController: Handles requests related to comparing price plans and recommending the best option.

ElectricityReading: Represents a single electricity reading (timestamp and value).

MeterReadings: Represents a collection of electricity readings for a specific smart meter.

PricePlan: Represents an energy price plan, including unit rate and peak time multipliers.

Supplier: Enum of available energy suppliers.

ElectricityReadingGenerator: Generates sample electricity readings for testing.

AccountService: Manages the association between smart meters and their assigned energy suppliers (price plans). Uses an in-memory dictionary for storage.

MeterReadingService: Manages the storage and retrieval of electricity readings. Uses an in-memory dictionary for storage (data is not persistent across application restarts).

PricePlanService: Calculates the cost of electricity consumption based on meter readings and price plans.

Startup: Configures the ASP.NET Core application, including services, dependency injection, and the request processing pipeline. Also includes hardcoded data for smart meter to price plan mappings and generates sample meter readings.

Program: Main entry point for application

5. Improvements and Fixes <a name="improvements"></a>

The provided code had several areas that needed improvement for robustness, correctness, and maintainability. Key changes made include:

MeterReadingService.GetReadings:

Corrected the logic to return readings for the requested smartMeterId, instead of always returning readings for "smart-meter-2".

Returns a new, sorted list of readings to prevent modification of the stored data.

PricePlanService.calculateCost:

Critical Fix: The original logic did not correctly calculate the cost by considering the time of each reading and the applicable peak/off-peak rates. The corrected logic now:

Sorts readings by time.

Iterates through consecutive pairs of readings.

Calculates the energy consumed during each interval.

Retrieves the correct price (including peak time multipliers) for the start of each interval using PricePlan.GetPrice.

Calculates the cost for each interval and accumulates the total cost.

MeterReadingController.Post:

Added input validation using IsMeterReadingsValid to check for null values and empty lists. Returns a 400 Bad Request if validation fails.

MeterReadingController.IsMeterReadingsValid:

Improved the input validation by directly check properties of meterReadings for null and emptiness

PricePlanComparatorController:

Added handling for cases where no readings are available for a given smart meter, returning a 404 Not Found response.

Startup.GenerateMeterElectricityReadings:

The ElectricityReadingGenerator now produces readings where each subsequent reading is larger than the previous one, simulating actual meter behavior, and the generated time sequence is incremental.

Comments

Added/Improved comments across the code base

Redundant Code

Removed redundant and commented out code

These changes significantly improve the accuracy and reliability of the application, addressing the core issues identified in the original code. The application is now in a much better state to handle both the existing requirements and future development.
