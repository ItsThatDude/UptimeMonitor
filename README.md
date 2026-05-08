[![Lint](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/lint.yml/badge.svg)](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/lint.yml) [![Tests](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/tests.yml/badge.svg)](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/tests.yml) [![Build](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/docker.yml/badge.svg)](https://github.com/ItsThatDude/UptimeMonitor/actions/workflows/docker.yml)
# Uptime Monitor

UptimeMonitor is a modular service monitoring application, powered by .NET Core and Angular

## Features

- Uses OAuth for authentication
- Remote Monitors (Workers)
- Monitors currently include Http, Tcp, Ping

## Local development

### Requirements

- .NET 8 SDK
- Node 22.x
- npm (for Nx) and npx
- Docker & Docker Compose (optional for containerized runs)
- Recommended: VS Code with Nx Console

### Backend (Web API)

1. Restore and build:  
  `dotnet build UptimeMonitor.sln`
2. Run the API:  
  `dotnet run --project src/UptimeMonitor.Web`

### Monitor service

- Run similarly:  
  `dotnet run --project src/UptimeMonitor.Service.MonitorService`

### Angular UI (development server)

From repo root:

```sh
npm ci
npx nx serve UptimeMonitor.Web.UI.Angular
```

### Running with Docker

- There are multi-stage Dockerfiles for web and monitor:
  - [docker/web.dockerfile](docker/web.dockerfile) (builds API, plugins, and UI)
  - [docker/monitor.dockerfile](docker/monitor.dockerfile)
- Build & run with Compose:  
  `docker-compose up --build`
- The web Dockerfile builds the Angular UI using an `ui-build` stage and copies the built files into the ASP.NET wwwroot (see [docker/web.dockerfile](docker/web.dockerfile)).

### Quick commands

- Build solution:  
  `dotnet build UptimeMonitor.sln`
- Run API + monitor locally:  

  ```sh
  dotnet run --project src/UptimeMonitor.Web
  dotnet run --project src/UptimeMonitor.Service.MonitorService
  ```

- Run frontend:  

  ```sh
  npm ci
  npx nx serve UptimeMonitor.Web.UI.Angular
  ```

- Docker:  
  `docker-compose up --build`
