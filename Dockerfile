FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Если здесь этого не указать:: error CS5001: Program does not contain a static 'Main' method suitable for an entry point [/HelloApi.csproj]
WORKDIR /src 

COPY *.csproj .
RUN dotnet restore

COPY . .

# Кэширует сборку?, если код изменился, а .csproj нет, то перейдёт сразу к publish
# RUN dotnet build "HelloApi.csproj" -c Release -o /app/build
RUN dotnet publish "HelloApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV LOGGING__CONSOLE__DISABLECOLORS=true

ENV ASPNETCORE_URLS=http://+:5285
EXPOSE 5285
ENTRYPOINT ["dotnet", "HelloApi.dll"]