# Build the Angular Web UI
FROM node:22.14.0-alpine AS ui-build
WORKDIR /ui
COPY ["src/UptimeMonitor.Web.UI.Angular/package*.json", "./"]
COPY ["src/UptimeMonitor.Web.UI.Angular/nx.json", "./"]
COPY ["src/UptimeMonitor.Web.UI.Angular/tsconfig.base.json", "./"]
COPY ["src/UptimeMonitor.Web.UI.Angular/eslint.config.mjs", "./"]
COPY ["src/UptimeMonitor.Web.UI.Angular/shared/", "./shared/"]
COPY ["src/UptimeMonitor.Web.UI.Angular/apps/UptimeMonitor.Web.UI.Angular/", "./apps/UptimeMonitor.Web.UI.Angular/"]
COPY ["src/UptimeMonitor.Web.UI.Angular/apps/UptimeMonitor.Web.UI.Angular-e2e/", "./apps/UptimeMonitor.Web.UI.Angular-e2e/"]
RUN npm ci
RUN npm install -g nx
ENV NX_DAEMON=false
RUN nx build --project UptimeMonitor.Web.UI.Angular

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# iputils-ping is required for the internal monitor
# curl is required when using docker healthchecks
RUN apt-get update && apt-get install -y iputils-ping curl
RUN apt-get clean && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

# This stage is used to build the web project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["src/UptimeMonitor.Web/UptimeMonitor.Web.csproj", "UptimeMonitor.Web/"]
COPY ["src/UptimeMonitor.Core.Domain/UptimeMonitor.Core.Domain.csproj", "UptimeMonitor.Core.Domain/"]
COPY ["src/UptimeMonitor.Core.Contracts/UptimeMonitor.Core.Contracts.csproj", "UptimeMonitor.Core.Contracts/"]
COPY ["src/UptimeMonitor.Service.MonitorService/UptimeMonitor.Service.MonitorService.csproj", "UptimeMonitor.Service.MonitorService/"]
COPY ["src/UptimeMonitor.Core.Plugins.PluginBase/UptimeMonitor.Core.Plugins.PluginBase.csproj", "UptimeMonitor.Core.Plugins.PluginBase/"]
COPY ["src/UptimeMonitor.Core.Plugins.PluginLoader/UptimeMonitor.Core.Plugins.PluginLoader.csproj", "UptimeMonitor.Core.Plugins.PluginLoader/"]
RUN dotnet restore --no-cache "./UptimeMonitor.Web/UptimeMonitor.Web.csproj"

COPY "src/UptimeMonitor.Web" "UptimeMonitor.Web"
COPY "src/UptimeMonitor.Core.Domain" "UptimeMonitor.Core.Domain"
COPY "src/UptimeMonitor.Core.Contracts" "UptimeMonitor.Core.Contracts"
COPY "src/UptimeMonitor.Service.MonitorService" "UptimeMonitor.Service.MonitorService"
COPY "src/UptimeMonitor.Core.Plugins.PluginBase" "UptimeMonitor.Core.Plugins.PluginBase"
COPY "src/UptimeMonitor.Core.Plugins.PluginLoader" "UptimeMonitor.Core.Plugins.PluginLoader"

RUN dotnet build "./UptimeMonitor.Web/UptimeMonitor.Web.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/build

# Build the monitoring plugins into the image

COPY ["src/UptimeMonitor.Monitors.PingMonitor/UptimeMonitor.Monitors.PingMonitor.csproj", "UptimeMonitor.Monitors.PingMonitor/"]
RUN dotnet restore --no-cache "./UptimeMonitor.Monitors.PingMonitor/UptimeMonitor.Monitors.PingMonitor.csproj"
COPY "src/UptimeMonitor.Monitors.PingMonitor" "UptimeMonitor.Monitors.PingMonitor"
RUN dotnet build "./UptimeMonitor.Monitors.PingMonitor/UptimeMonitor.Monitors.PingMonitor.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/build/plugins

COPY ["src/UptimeMonitor.Monitors.HttpMonitor/UptimeMonitor.Monitors.HttpMonitor.csproj", "UptimeMonitor.Monitors.HttpMonitor/"]
RUN dotnet restore --no-cache "./UptimeMonitor.Monitors.HttpMonitor/UptimeMonitor.Monitors.HttpMonitor.csproj"
COPY "src/UptimeMonitor.Monitors.HttpMonitor" "UptimeMonitor.Monitors.HttpMonitor"
RUN dotnet build "./UptimeMonitor.Monitors.HttpMonitor/UptimeMonitor.Monitors.HttpMonitor.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/build/plugins

COPY ["src/UptimeMonitor.Monitors.TCPMonitor/UptimeMonitor.Monitors.TCPMonitor.csproj", "UptimeMonitor.Monitors.TCPMonitor/"]
RUN dotnet restore --no-cache "./UptimeMonitor.Monitors.TCPMonitor/UptimeMonitor.Monitors.TCPMonitor.csproj"
COPY "src/UptimeMonitor.Monitors.TCPMonitor" "UptimeMonitor.Monitors.TCPMonitor"
RUN dotnet build "./UptimeMonitor.Monitors.TCPMonitor/UptimeMonitor.Monitors.TCPMonitor.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/build/plugins

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./UptimeMonitor.Web/UptimeMonitor.Web.csproj" --no-restore -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish /p:UseAppHost=false

RUN dotnet publish "./UptimeMonitor.Monitors.PingMonitor/UptimeMonitor.Monitors.PingMonitor.csproj" --no-restore -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish/plugins/PingMonitor /p:UseAppHost=false
RUN dotnet publish "./UptimeMonitor.Monitors.HttpMonitor/UptimeMonitor.Monitors.HttpMonitor.csproj" --no-restore -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish/plugins/HttpMonitor /p:UseAppHost=false
RUN dotnet publish "./UptimeMonitor.Monitors.TCPMonitor/UptimeMonitor.Monitors.TCPMonitor.csproj" --no-restore -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish/plugins/TcpMonitor /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=ui-build /ui/dist/apps/UptimeMonitor.Web.UI.Angular/browser ./wwwroot
ENTRYPOINT ["dotnet", "UptimeMonitor.Web.dll"]