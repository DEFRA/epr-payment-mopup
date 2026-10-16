# Local container regression test

Run from the repository root with Docker Desktop running and Python 3 installed:

```bash
python3 tests/local-container/run.py
```

The runner builds the actual function Dockerfile as Linux amd64, starts a disposable SQL Server
2022 Developer instance, Azurite and a deterministic GOV.UK Pay HTTP stub, seeds 15 payment
cases, then starts the unmodified function image. On Apple Silicon the function and SQL Server
run under amd64 emulation. SQL Server Developer is for development/test use only.

All fixture containers use an internal Docker network with no published ports. The connection
strings, token and passwords in compose.yaml are public local test values, not credentials for
any external service. No Azure account, real GovPay token, deployment or pipeline is used.

The runner verifies:

- Isolated worker startup on .NET 10 under the image's non-root `dotnet` user.
- Discovery of MopUpPaymentsFunction and at least two completed timer invocations.
- Local JSON settings load and environment variables override their values.
- SQL reads and writes using EF Core 10 and the SQL Server provider.
- Success, failed, P0030 user-cancelled and error mappings, including persisted error details.
- Successful processing after earlier API errors in the same batch.
- Nonfinal, HTTP 500, malformed and missing-state responses leave payments unchanged and
  are retried in the next timer cycle.
- Initiated/already-final payments, rows outside the 180–270 minute fixture window and
  missing/empty GovPay IDs are untouched and never sent to the HTTP stub.
- Completed payments are not queried from GovPay again during the second cycle.
- Application Information/error messages are present in container logs.

Results and logs are written to the ignored `artifacts/` directory. The runner removes its
Compose containers/network and disposable database in `finally`, including after a test fails.
If the runner is forcibly killed, clean up only this project with:

```bash
docker compose -p mopup-net10-local -f tests/local-container/compose.yaml down --volumes --remove-orphans
```

A successful run is not evidence of Azure deployment, real GOV.UK Pay sandbox compatibility,
Application Insights ingestion or compatibility with the externally managed database schema.
See [upgrade validation](../../docs/net10-upgrade-validation.md) for the remaining regression checklist. The fixture uses
an accelerated 10-second schedule and synthetic schema/responses; real schedule, identity,
TLS/network configuration and coupled frontend/facade flows still need environment testing.
