# epr-payment-mopup


## Description
Scheduled Azure Function that reconciles unfinished EPR online payments with GOV.UK Pay and records final results in the payments database.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Azure Functions Core Tools v4 with .NET 10 isolated worker support
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
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FUNCTIONS_TIME_TRIGGER": "0 0 * * * *",
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



### Validation

From the repository root:

```bash
dotnet restore src/EPR.Payment.Mopup.sln
dotnet build src/EPR.Payment.Mopup.sln --configuration Release --no-restore
dotnet test src/EPR.Payment.Mopup.sln --configuration Release --no-build
dotnet list src/EPR.Payment.Mopup.sln package --vulnerable --include-transitive
docker build -f src/EPR.Payment.Mopup.Function/Dockerfile src
```

Existing AutoMapper and FluentAssertions versions are unchanged.
See [upgrade validation and remaining checks](docs/net10-upgrade-validation.md).
The function uses the isolated worker timer extension and worker Application Insights integration.
Worker logging is configured separately from host logging in `host.json`; the SDK's default
Warning-only Application Insights filter is removed to preserve application Information logs.

### Deployment and regression checks

CI and deployment use `DEFRA-COMMON-UBUNTU-SSV3`, matching
[epr-pom-func-producer-validation (FAx402)](https://github.com/DEFRA/epr-pom-func-producer-validation/tree/main/pipelines).
CI selects `dotnetVersion10` in the shared build template, which discovers all three unit test projects.
The [completed convention review](docs/net10-upgrade-validation.md#pipeline-agentsdk-configuration-matching-fax402--complete)
compares these settings with FAx402's SUB-276 upgrade commit `fa81c40` (PR #92).

Coordinate these app settings in the environment configuration repository with the first isolated deployment:

- Set `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated` and retain `FUNCTIONS_EXTENSION_VERSION=~4`.
- Remove `FUNCTIONS_INPROC_NET8_ENABLED` if present. Azure app settings override Docker image defaults.
- Retain the existing `FUNCTIONS_TIME_TRIGGER`, storage, SQL connection, GovPay configuration,
  reconciliation windows and Application Insights connection string.

The shared deployment template updates the container image; it does not migrate those app settings.
The Docker image also sets `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated` for local/container defaults.

In a test environment, confirm that `MopUpPaymentsFunction` is discovered, the configured timer fires,
and Information/error telemetry reaches Application Insights. With GOV.UK Pay sandbox payments and
the test payments database, verify successful, failed, cancelled (including P0030) and expired outcomes
are recorded as before; nonfinal outcomes remain unchanged and an error for one payment does not
prevent subsequent payments being processed. Verify only InProgress rows within the configured
age window with a GovPay ID are selected, with Initiated and older-than-window rows left unchanged.
Also exercise the payment frontend/facade callback path against the same test service/database.
No payment selection, status mapping or window rules are changed by this migration.
