# epr-payment-mopup


## Description
Service to calculate fees and manage payment records for EPR

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio or Visual Studio Code
- MSSQL
- Azurite - https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage#install-azurite
- Application Insights API Key
- GovPayService API Key

### Installation
1. Clone the repository:
    ```bash
    git clone https://github.com/DEFRA/epr-payment-mopup.git
    ```
2. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Mopup.Function
    ```
3. Restore the dependencies:
    ```bash
    dotnet restore
    ```

### Configuration
The application uses local.settings.json for configuration.

- Replace the APPLICATIONINSIGHTS_CONNECTION_STRING below with your own

- Replace the BearerToken below with your GovPayService API Key

#### Sample 
local.settings.json

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_INPROC_NET8_ENABLED": "1",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "FUNCTIONS_TIME_TRIGGER": "0 */90 * * * *",
    "APPLICATIONINSIGHTS_CONNECTION_STRING": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "SqlConnectionString": "Data Source=.;Initial Catalog=FeesPayment;Trusted_Connection=true;TrustServerCertificate=true;",
    "TotalMinutesToUpdate": "270",
    "IgnoringMinutesToUpdate": "180"
  },
  "Services": {
    "GovPayService": {
      "Url": "https://publicapi.payments.service.gov.uk",
      "EndPointName": "v1",
      "BearerToken": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

### Building the Application
1. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Mopup.Function
    ```

2. To build the application:
    ```bash
    dotnet build
    ```

### Running the Application
1. Navigate to the project directory:
    ```bash
    cd \src\EPR.Payment.Mopup.Function
    ```
 
2. To run the service locally:
    ```bash
    dotnet run
    ```

3. The Function will run based off the FUNCTIONS_TIME_TRIGGER timer in the configuration, this can be shortened to 30 seconds by replacing it with "*/30 * * * * *" 

