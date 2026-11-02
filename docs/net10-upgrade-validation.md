# FAx409 .NET 10 upgrade validation

Validated on 16 October 2026 with .NET SDK 10.0.401.

## Changes

All six application/test projects target net10.0. The function is an isolated worker executable with
Program.cs startup, worker Application Insights and timer bindings. Its function name and
`%FUNCTIONS_TIME_TRIGGER%` setting are unchanged. PaymentsService, PaymentsRepository,
PaymentStatusMapper and mapping profiles are unchanged.

Docker uses the .NET 10 SDK and `mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0`.
Both pipeline pools and the CI SDK selector match FAx402's main-branch configuration:
`DEFRA-COMMON-UBUNTU-SSV3` and `dotnetVersion10`. Shared build templates discover all three unit test projects.

## Pipeline agent/SDK configuration matching FAx402 — complete

Reviewed the local reference repository's upgrade commit
[`fa81c4074e076e703f959e99d8571e2383b38549`](https://github.com/DEFRA/epr-pom-func-producer-validation/commit/fa81c4074e076e703f959e99d8571e2383b38549),
**Feature/sub 276 upgrade to dotnet10 (#92)**, dated 17 June 2026 (SUB-276).
Both its upgrade diff and current checked-out pipeline configuration were inspected read-only.

| Convention from FAx402's upgrade | FAx409 configuration | Review result |
| --- | --- | --- |
| CI pool `DEFRA-COMMON-UBUNTU-SSV3` | `pipelines/ci_pipeline.yaml` | Exact match |
| Deployment pool `DEFRA-COMMON-UBUNTU-SSV3` | `pipelines/deployment-pipeline.yaml` | Exact match |
| CI variable `dotnetVersion: dotnetVersion10` | `variables` entry in CI | Exact match |
| Forward SDK selector as `dotnetVersion: ${{ variables.dotnetVersion }}` | CI `extends.parameters` | Exact match; avoids the variable-name typo corrected in FAx402 |
| Shared `epr-build-pipeline.yaml@CommonTemplates` from `defra/epr-webapps-code-deploy-templates`, ref `main` | CI resources and extends | Exact match |
| Shared `epr-funcs-deployment-pipeline.yaml@CommonTemplates` | Deployment extends | Exact match |
| SDK `mcr.microsoft.com/dotnet/sdk:10.0` and runtime `mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0` | Function Dockerfile | Exact match |
| `Microsoft.NET.Sdk`, Functions v4, executable output, Worker.Sdk 2.0.7 | Function project | Same isolated project convention |

A local assertion script compared these pipeline values against `git show` at the upgrade commit;
all checks passed. The SDK selector is forwarded to the shared template rather than adding a
repo-specific SDK installation task, matching FAx402. The shared SDK script inspected earlier selects
.NET 10 using `global.json`; the updated agent pool supplies the SDK. All three FAx409 unit test
projects match the shared template's test discovery pattern.

FAx402 was already isolated before SUB-276, so it did not need our in-process Startup/timer migration.
Its Worker package at that commit was 2.2.0; FAx409 uses the later .NET 10 worker 2.52.0 and timer
extension, while retaining the same Worker.Sdk convention. No Service Bus/HTTP bindings,
integration-test pipeline toggle, later lockfile/OSV work, environment removals or service-connection
changes were copied from FAx402: those are unrelated to this timer function upgrade.

**Configuration review is complete. Pipeline execution remains unverified and was intentionally not
attempted. No deployment or pipeline is authorized by these checks.**

## Complete FAx402 conversion-history comparison

Rechecked the complete .NET 10 diff `fa81c4074e076e703f959e99d8571e2383b38549`
(SUB-276, PR #92, 17 June 2026), plus the earlier merged isolated-worker migration
`fb99a2d` (SMAL-318, PR #72, 25 February 2026). Also inspected its preliminary branch
commits `079e20d` and `770da5d`; these contain the same worker/bootstrap and isolated-image
changes subsequently included in the merged migration.

Every file changed by SUB-276 was checked:

| FAx402 change | FAx409 equivalent or reason it does not apply |
| --- | --- |
| CI agent, .NET SDK selector and corrected parameter forwarding | Exact same pool, selector and forwarding; see table above |
| Deployment agent | Same repository-level pool; shared template still controls its own job pools |
| `Directory.Build.props` target framework | FAx409 has no shared props file; all six existing per-project targets are net10.0 instead of introducing a new build structure |
| Application project target and `Microsoft.Extensions.*` packages | Common/Data projects target net10.0. Extensions dependencies use the .NET 10 packages/shared framework; Common uses Microsoft.AspNetCore.App for its existing HTTP abstractions |
| Azure.Identity/Azure.Core updates | Azure.Identity is updated to 1.21.0; Azure.Core resolves transitively to 1.53.0. Existing SQL/EF dependencies are also updated for this service |
| Function executable, Worker and Worker.Sdk packages | Functions v4 executable, Worker 2.52.0, Worker.Sdk 2.0.7. SDK version matches; the later Worker satisfies current .NET 10 guidance rather than downgrading to the reference commit's 2.2.0 |
| HTTP and Service Bus binding updates | This service only has a timer trigger, so it uses the isolated Timer extension and preserves its existing name/schedule setting |
| SDK Docker image | Exact match: mcr.microsoft.com/dotnet/sdk:10.0 |
| Isolated runtime Docker image | Exact match: mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 |
| Docker build/publish/copy pattern | Same two-stage build, dotnet publish and /home/site/wwwroot output; explicit Release is retained (also the .NET 10 publish default) |
| Docker non-root user and Functions environment | Same group/user creation, ownership, USER dotnet, script root and console logging; FAx409 additionally supplies the isolated runtime default, which Azure app settings can override |
| Application and Function test project targets | All three test projects target net10.0; timer test types use the worker extension |
| Remove legacy System.Net.Http and RegularExpressions packages | Removed from both test projects that referenced them; framework-provided System.Text.Json references also removed |
| IntegrationTests/TestSupport/UnitTest targets | FAx409 has no corresponding extra projects; all existing solution projects were checked |
| CSV fixture path comment net8.0 → net10.0 | FAx409 has no CSV regression loader or corresponding hardcoded framework path |
| FeatureManagement, System.Drawing and HTTP resilience packages | Not dependencies of this timer job; adding feature flags, drawing, retries or HTTP endpoints would change scope/behavior |

The earlier isolated migration was also checked:

- Both replace FunctionsStartup and the in-process SDK with `HostBuilder`,
  `ConfigureFunctionsWorkerDefaults`, `ConfigureServices`, `Build` and `Run` in Program.cs.
  FAx409 now follows the same explicit static Program/Main and ExcludeFromCodeCoverage convention.
- Both use worker Function attributes and constructor-injected typed logging. FAx409 already had
  constructor logging; only its timer attribute/types needed replacement.
- Both use ApplicationInsightsTelemetryWorkerService. FAx409 also configures the isolated worker's
  Application Insights integration and removes its default Warning-only filter to retain Information logs.
- Both copy local.settings.json on every local build and exclude it from publish. FAx409 preserves
  its existing optional local/environment JSON and user-secret loading, with environment variables last.
- FAx402 publishes appsettings.json containing its submission-period reference data. FAx409 has no
  corresponding static reference data/file; it retains its existing settings contract instead of adding
  or renaming configuration sources. Working-directory lookup is retained to avoid the regression
  reproduced during local review.
- FAx402's HTTP test endpoint, Postman collection, feature flag, submission-period data and validator
  changes do not apply to this timer/payment service. Its existing IDE-local DLL HintPath and stale
  commented Docker alternatives are not portable upgrade conventions and are not copied.
- AutoMapper and payment behavior remain unchanged.

The Dockerfile change was already included in the original FAx409 upgrade commit `7f89e54` and
PR #57; this follow-up recheck does not identify a missing image-version switch.

## Companion app-configuration change required before rollout

The application pipeline references Azure DevOps repository
`RWD-CPR-EPR4P-ADO/epr-app-config-settings`. The inspected GitHub counterpart
`DEFRA/epr-app-config-settings` contains these FAx409 configuration files:

| Environment | Configuration path |
| --- | --- |
| Shared development template, including DEV4 | dev/DEVRWDWEBFAX409.json |
| DEV8 | dev8/DEVRWDWEBFA8409.json |
| DEV15 | dev15/DEVRWDWEBFAF409.json |
| Test | tst1/TSTRWDWEBFA1409.json |
| Preproduction | pre1/PRERWDWEBFA1409.json |
| Preproduction 2 | pre2/PRERWDWEBFA2409.json |
| Production | prd1/PRDRWDWEBFA1409.json |

All seven inspected files contain FUNCTIONS_WORKER_RUNTIME=dotnet and
FUNCTIONS_INPROC_NET8_ENABLED=1. In the authoritative configuration, set the former to
dotnet-isolated and remove the latter, coordinated with each environment's image rollout.
Retain FUNCTIONS_EXTENSION_VERSION=~4 where configured; it is not explicitly present in these
seven files. Keep SQL/storage/GovPay settings, schedule, windows and the container hosting model.

The GitHub repository history contains ADO refresh commits; its FAx402 settings file has not changed
since 2025 and does not explicitly set either runtime flag. That is not evidence of current live
Azure settings or of how the 2026 FAx402 runtime configuration was applied. Verify the authoritative
ADO revision and current app settings before making/applying its companion change. No authenticated
Azure session was available for this inspection.

The existing `pipelines/pipeline_single_configuration.yaml` materializes the development template
and applies the JSON through `az webapp config appsettings set`; it can restart the app. This is a
separate configuration operation from the image-only application deployment. Neither operation
was run, and no app-config files were edited. Changing the runtime setting on the old image would
create a temporary runtime mismatch, so coordinate both changes in the authorized rollout.

## Completed checks

- Solution restore and Release build succeeded.
- All 69 tests passed: Common 48, Data 6, Function 15 (including a new dependency-resolution smoke test).
- Tests also passed using the shared pipeline's AzureBuild/CollectCoverage/OpenCover arguments;
  coverage reports were generated for all three test projects.
- Release publish succeeded. Published runtime configuration targets net10.0; function and worker
  metadata identify dotnet-isolated and preserve the timer name and schedule setting.
- Microsoft Container Registry returned the selected runtime tag's Linux amd64 manifest.
- Direct/transitive NuGet audits covered the solution and the generated WorkerExtensions project.
  The only remaining reported vulnerable package is the existing AutoMapper 13.0.1 dependency.
- Actual Linux amd64 Docker build and container startup passed. The isolated worker ran as the
  non-root `dotnet` user with Microsoft.NETCore.App/Microsoft.AspNetCore.App 10.0.12 supplied by
  the selected Functions image (amd64 emulation on Apple Silicon).
- Two completed timer invocations passed against disposable local SQL Server 2022, Azurite and
  a GOV.UK Pay HTTP stub. All 15 seeded cases passed: success, failure, P0030 cancellation,
  error, nonfinal, HTTP 500, malformed JSON, missing state, success after earlier errors,
  Initiated, too recent, too old, empty/null GovPay IDs, and already-final status.
- Persisted status/error details and update timestamps matched expectations. Unresolved/error
  rows remained unchanged and were retried; completed rows were not called again next cycle.
  Information and expected fixture-error logs were visible. The fixture network was internal,
  with no published ports or live service credentials. Test containers/network/database were removed.
- All 69 unit tests were rerun successfully with coverage after container test work.
- Final review found and fixed JSON lookup behavior introduced by `new HostBuilder()`: explicit
  `SetBasePath(Directory.GetCurrentDirectory())` preserves the old Startup's working-directory lookup.
  A local executable probe reproduced the failure before the fix. The container harness additionally
  checks JSON settings and an environment-variable override.
- The Docker context excludes host build outputs, test artifacts and local settings, keeping local
  image builds reproducible and avoiding copying development settings into build layers.
- No payment selection, reconciliation-window or status-mapping code changed.

This was a one-off local run; the harness used for it is not part of the repository.

## Dependency security and licensing

A new SqlClient dependency chain resolved System.Security.Cryptography.Pkcs 9.0.11. A direct
10.0.13 reference in Common.Data overrides it to address
[CVE-2026-70162](https://github.com/advisories/GHSA-2fmv-rv2c-59x7) and
[CVE-2026-84008](https://github.com/advisories/GHSA-846f-f764-76mx).
The final audits report neither vulnerability and no findings in generated worker extensions.

AutoMapper 13.0.1, its mapping configuration and FluentAssertions 6.x remain unchanged.
The audit still reports the pre-existing AutoMapper advisory
[CVE-2026-32933](https://github.com/advisories/GHSA-rvv3-g6hj-g44x); no AutoMapper changes are
included in this ticket.

## Final PR scope review

No remaining upgrade-specific blocking findings after the following review fixes:

- Preserved the original working-directory JSON lookup when replacing in-process Startup.
- Rechecked all FAx402 conversion files; aligned explicit Program/Main, startup coverage exclusion
  and local-settings copy behavior with its isolated migration.
- Verified JSON configuration and environment override behavior in the container harness.
- Excluded host build/test artifacts and development settings from the Docker context; the final
  container image was rebuilt from that clean context and all 15 fixture cases passed again.
- Removed the internal implementation plan from the proposed PR and preserved existing file encodings.
- Verified AutoMapper package versions and mappings are unchanged; no replacement or mitigation is included.
- Checked the actual FAx402 SUB-276 history and completed the pipeline comparison above.

Local Release tests (69/69), publish metadata checks, the two-cycle container harness and a
Gitleaks scan of changed source files passed. Dependency audits report only the existing unchanged
AutoMapper finding. Payment services, repository selection queries, status mapping and mapping profiles
are unchanged from HEAD. No external repository, service connection, environment inventory or
payment behavior changes are included. Test containers/network/database have been removed.

Ready for PR review, with pipeline execution and environment regression explicitly outstanding below.

## Regression testing still required — no deployment authorized

No deployment or pipeline was triggered. No Azure resources or external repositories were modified.
The local fixture uses a synthetic schema matching this repository's model and deterministic HTTP
responses. It verifies SQL integration, but does not prove compatibility with the actual payments
service's managed database schema or the real GOV.UK Pay API. Local console logs do not prove
Application Insights ingestion. The accelerated timer does not establish production-scale performance.

The following checks remain for a separately authorized test-environment exercise:

1. **Real payment flow:** use epr-payment-frontend → facade → service with GOV.UK Pay sandbox.
   Complete the callback normally, then abandon a separate payment before callback. Confirm the
   callback and mopup produce the same final statuses/error details without corrupting either row.
2. **Real GOV.UK Pay outcomes:** exercise success, failed/declined, P0030 user cancellation,
   expiry/error and nonfinal responses with real sandbox payloads, authentication and HTTPS.
3. **Database compatibility:** use the actual payment-service schema, relationships, permissions
   and SQL version/compatibility level. Check that existing records and unrelated fields remain
   correct; exercise transient DB failures and callback/mopup overlap. No schema migration is included.
4. **Selection/window boundaries:** verify real configured schedule/windows, including rows at
   the 180- and 270-minute boundaries, missing IDs, Initiated/final statuses and older rows. The
   local tests cover clearly inside/outside values, not exact moving-clock boundary instants.
5. **Azure hosting/configuration:** verify the isolated runtime setting, storage lease/timer
   behavior, identity/network/TLS access, restart behavior, and absence of duplicate processing
   across host instances using the intended hosting plan and environment settings.
6. **Telemetry:** confirm function traces, Information/error messages, exceptions and dependency
   telemetry arrive in Application Insights with expected correlation and filtering.
7. **Operational behavior:** test realistic batch sizes, API throttling/timeouts and prolonged
   outages. Validate duration/memory against the previous service; no performance improvement
   has been measured locally.

When a deployment is separately approved, coordinate `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`
in environment app settings, retain `FUNCTIONS_EXTENSION_VERSION=~4`, and remove any
`FUNCTIONS_INPROC_NET8_ENABLED` setting. Environment settings override Docker defaults; the shared
deployment template does not migrate them. Retain the existing schedule, payment windows and
service connection settings. The build/deployment pipelines themselves remain unexecuted.

## References

- [Microsoft isolated worker guidance](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [FAx402 pipeline configuration](https://github.com/DEFRA/epr-pom-func-producer-validation/tree/main/pipelines)
- [Shared build and deployment templates](https://github.com/DEFRA/epr-webapps-code-deploy-templates)
