FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src 

COPY *.csproj .
RUN dotnet restore

COPY . .

RUN dotnet publish "HelloApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV LOGGING__CONSOLE__DISABLECOLORS=true

ENV ASPNETCORE_URLS=http://+:5285
EXPOSE 5285
ENTRYPOINT ["dotnet", "HelloApi.dll"]