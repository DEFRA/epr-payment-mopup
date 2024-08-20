FROM defradigital/dotnetcore-development:dotnet8.0 AS base
 
USER root
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}
 
RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
 
FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root
 
# Install dotnet-ef tool
RUN dotnet tool install dotnet-ef --version 8.* --tool-path "/root/.dotnet/tools"
 
# Add the tools folder to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"
 
WORKDIR /src
COPY ["EPR.Payment.Mopup/EPR.Payment.Mopup.csproj", "EPR.Payment.Mopup/"]
COPY ["EPR.Payment.Mopup.Common.Data/EPR.Payment.Mopup.Common.Data.csproj", "EEPR.Payment.Mopup.Common.Data/"]
COPY ["EPR.Payment.Mopup.Common/EPR.Payment.Mopup.Common.csproj", "EPR.Payment.Mopup.Common/"]

RUN dotnet restore "./EPR.Payment.Mopup/EPR.Payment.Mopup.csproj"

COPY --chown=dotnet:dotnet EPR.Payment.Mopup/. ./EPR.Payment.Mopup/.
COPY --chown=dotnet:dotnet EPR.Payment.Mopup.Common.Data/. ./EPR.Payment.Mopup.Common.Data/.
COPY --chown=dotnet:dotnet EPR.Payment.Mopup.Common/. ./EPR.Payment.Mopup.Common/.

WORKDIR "/src/EPR.Payment.Mopup"
RUN dotnet build "./EPR.Payment.Mopup.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EPR.Payment.Mopup.csproj" -c Release /p:AzureBuild=false -o /app/publish

FROM base AS final
USER dotnet
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EPR.Payment.Mopup.dll"]
