FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Copy everything
COPY NuGet.Config ./

COPY EPR.Payment.Mopup.Common/. ./EPR.Payment.Mopup.Common/.
COPY EPR.Payment.Mopup.Common.Data/. ./EPR.Payment.Mopup.Common.Data/.
COPY EPR.Payment.Mopup.Function/. ./EPR.Payment.Mopup.Function/.

# Publish 
RUN dotnet publish EPR.Payment.Mopup.Function/*.csproj --output /home/site/wwwroot

# Use the Azure Functions image as the final image
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]