# Migration Guide: MSSQL to PostgreSQL & .NET to .NET Core with Docker

Quick reference guide for migrating from MSSQL/.NET Framework to PostgreSQL/.NET Core 8.0 with Docker deployment.

---

## 1. Project Setup

### Initialize .NET Core Projects

```bash
# Backend API
mkdir Backend && cd Backend
dotnet new webapi -n DashboardApi
cd ..

# Frontend Web App
mkdir Frontend && cd Frontend
dotnet new webapp -n DashboardWeb
cd ..
```

---

## 2. Backend Configuration

### Install PostgreSQL Packages (Replace MSSQL)

```bash
cd Backend

# Remove MSSQL packages (if migrating)
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer

# Add PostgreSQL packages
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
```

### Update DbContext for PostgreSQL

**Key Changes from MSSQL:**

```csharp
// OLD (MSSQL)
options.UseSqlServer(connectionString);

// NEW (PostgreSQL)
options.UseNpgsql(connectionString);
```

### Connection String Format Change

```bash
# OLD - MSSQL
Server=localhost;Database=mydb;User Id=sa;Password=pass;

# NEW - PostgreSQL
Host=postgres;Port=5432;Database=mydb;Username=postgres;Password=pass;
```

### Program.cs - Database Initialization

```csharp
// Auto-create database on startup (Development)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<YourContext>();
    db.Database.EnsureCreated(); // or db.Database.Migrate() for production
}
```

### Add CORS (For Frontend-Backend Communication)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Before app.Run()
app.UseCors("AllowAll");
```

---

## 3. Frontend Configuration

### Add HttpClient for API Calls

```csharp
// Program.cs
builder.Services.AddHttpClient();
```

### Configure Backend API URL

```json
// appsettings.json
{
  "BackendApi": {
    "Url": "http://backend:8080"
  }
}
```

### Call Backend from Razor Pages

```csharp
private readonly IHttpClientFactory _httpClientFactory;

public async Task OnGetAsync()
{
    var client = _httpClientFactory.CreateClient();
    var response = await client.GetAsync("http://backend:8080/api/dashboard");
    var data = await response.Content.ReadFromJsonAsync<List<YourModel>>();
}
```

---

## 4. Docker Configuration

### Backend Dockerfile

```dockerfile
# Backend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DashboardApi.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DashboardApi.dll"]
```

### Frontend Dockerfile

```dockerfile
# Frontend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DashboardWeb.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DashboardWeb.dll"]
```

### .dockerignore (Both Projects)

```
bin/
obj/
*.user
.vs/
.vscode/
```

---

## 5. Docker Compose Setup

### docker-compose.yml

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: dashboard-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: dashboarddb
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - dashboard-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ./Backend
      dockerfile: Dockerfile
    container_name: dashboard-backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=dashboarddb;Username=postgres;Password=postgres123
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - dashboard-network

  frontend:
    build:
      context: ./Frontend
      dockerfile: Dockerfile
    container_name: dashboard-frontend
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - BackendApi__Url=http://backend:8080
    ports:
      - "5000:5000"
    depends_on:
      - backend
    networks:
      - dashboard-network

networks:
  dashboard-network:
    driver: bridge

volumes:
  postgres_data:
```

---

## 6. Linux/Bash Commands

### Build and Run

```bash
# Build and start all services
docker-compose up --build -d

# View logs
docker-compose logs -f

# View specific service
docker-compose logs -f backend
docker-compose logs -f postgres
```

### Stop and Cleanup

```bash
# Stop services
docker-compose down

# Stop and remove volumes (fresh database)
docker-compose down -v

# Remove all unused Docker resources
docker system prune -a
```

### Restart Services

```bash
# Restart single service
docker-compose restart backend

# Rebuild single service
docker-compose up --build backend -d
```

### Database Access

```bash
# Connect to PostgreSQL CLI
docker exec -it dashboard-postgres psql -U postgres -d dashboarddb

# Common PostgreSQL commands
\dt                           # List tables
\d "TableName"                # Describe table
SELECT * FROM "TableName";    # Query data
\q                            # Quit
```

### Container Management

```bash
# List running containers
docker ps

# Execute command in container
docker exec -it dashboard-backend sh

# View container logs
docker logs dashboard-backend

# Check container resource usage
docker stats
```

---

## 7. Key Migration Differences

### MSSQL vs PostgreSQL

| Feature | MSSQL | PostgreSQL |
|---------|-------|------------|
| **Connection** | `Server=` | `Host=` |
| **Package** | `Microsoft.EntityFrameworkCore.SqlServer` | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **DbContext** | `UseSqlServer()` | `UseNpgsql()` |
| **Case Sensitivity** | Case-insensitive | Case-sensitive (use quotes for mixed case) |
| **Identity Column** | `IDENTITY(1,1)` | `SERIAL` or `GENERATED BY DEFAULT AS IDENTITY` |
| **DateTime** | `datetime2` | `timestamp with time zone` (use UTC) |
| **Boolean** | `bit` | `boolean` |

### .NET Framework vs .NET Core

| Feature | .NET Framework | .NET Core 8.0 |
|---------|----------------|---------------|
| **Runtime** | Windows only | Cross-platform (Linux, macOS, Windows) |
| **Web Framework** | ASP.NET MVC, Web Forms | ASP.NET Core (unified) |
| **Deployment** | IIS | Docker, Kestrel, Linux services |
| **Config** | `Web.config` | `appsettings.json` + Environment Variables |
| **DI** | Manual/Autofac | Built-in DI container |
| **Project File** | `.csproj` (verbose XML) | `.csproj` (SDK-style, minimal) |

---

## 8. Important Docker Concepts

### Service Communication

```bash
# WRONG - Using localhost inside containers
http://localhost:8080

# CORRECT - Using Docker service names
http://backend:8080
http://postgres:5432
```

### Environment Variables

```yaml
# Override appsettings.json with env vars
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;...
  - BackendApi__Url=http://backend:8080

# Double underscore __ replaces nested JSON structure
# ConnectionStrings:DefaultConnection → ConnectionStrings__DefaultConnection
```

### Health Checks

```yaml
# Ensure PostgreSQL is ready before starting backend
depends_on:
  postgres:
    condition: service_healthy

healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres"]
  interval: 10s
  retries: 5
```

### Volumes (Data Persistence)

```yaml
volumes:
  - postgres_data:/var/lib/postgresql/data

# Data persists even when containers are removed
# Delete with: docker-compose down -v
```

---

## 9. Common Issues & Fixes

### "relation does not exist" Error

```bash
# Database table not created - restart with fresh DB
docker-compose down -v
docker-compose up --build -d
```

### Frontend Can't Reach Backend

```bash
# Check backend is running
docker exec dashboard-frontend ping backend

# Verify environment variable
docker exec dashboard-frontend env | grep BackendApi
```

### Port Already in Use

```bash
# Find process using port (Linux)
sudo lsof -i :5000
sudo kill -9 <PID>

# Or change port in docker-compose.yml
ports:
  - "5001:5000"
```

### Database Connection Refused

```bash
# Check PostgreSQL health
docker-compose ps postgres

# Should show "Up (healthy)"
# If not, check logs
docker-compose logs postgres
```

### Migration Files Not Applying

```bash
# Option 1: Use EnsureCreated() instead of Migrate()
db.Database.EnsureCreated();

# Option 2: Recreate migrations
rm -rf Migrations/
dotnet ef migrations add InitialCreate
docker-compose up --build -d
```

---

## 10. PostgreSQL Specific Notes

### Case-Sensitive Table/Column Names

```csharp
// Use quotes for exact case matching
SELECT * FROM "DashboardItems";  // Correct
SELECT * FROM dashboarditems;     // Wrong (won't find table)
```

### DateTime Handling

```csharp
// Always use UTC for PostgreSQL
CreatedAt = DateTime.UtcNow;
CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

// NOT this
CreatedAt = DateTime.Now;  // Can cause timezone issues
```

### Connection Pooling

```bash
# Add to connection string for better performance
Host=postgres;Port=5432;Database=db;Username=user;Password=pass;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;
```

---

## 11. Production Deployment

### Use Migrations (Not EnsureCreated)

```csharp
// Development
db.Database.EnsureCreated();

// Production
db.Database.Migrate();
```

### Secure Secrets

```yaml
# DON'T commit passwords to git
# Use environment variables from secure store

# Docker Compose with env file
env_file:
  - .env

# Or use Docker secrets, Azure Key Vault, etc.
```

### Logging

```bash
# Add structured logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### Health Check Endpoints

```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

app.MapHealthChecks("/health");
```

---

## 12. Quick Command Reference

```bash
# Start application
docker-compose up -d

# View all logs
docker-compose logs -f

# Rebuild and restart
docker-compose down
docker-compose up --build -d

# Fresh database
docker-compose down -v
docker-compose up -d

# Access PostgreSQL
docker exec -it dashboard-postgres psql -U postgres -d dashboarddb

# Test API
curl http://localhost:8080/api/dashboard

# Stop everything
docker-compose down

# Clean Docker system
docker system prune -a -f
docker volume prune -f
```

---

## Summary

**Migration Checklist:**

- [x] Replace `Microsoft.EntityFrameworkCore.SqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL`
- [x] Update connection strings (Server → Host)
- [x] Change `UseSqlServer()` to `UseNpgsql()`
- [x] Handle PostgreSQL case-sensitivity (use quotes)
- [x] Use UTC DateTime for PostgreSQL timestamps
- [x] Add CORS for frontend-backend communication
- [x] Create Dockerfiles for both projects
- [x] Setup docker-compose.yml with health checks
- [x] Use Docker service names (not localhost)
- [x] Configure environment variables for container networking
- [x] Test with `docker-compose up --build -d`

**Access Points:**
- Frontend: http://localhost:5000
- Backend API: http://localhost:8080/api/dashboard
- Swagger: http://localhost:8080/swagger
- PostgreSQL: localhost:5432

---

**Team Migration Path:**
1. Update NuGet packages (MSSQL → PostgreSQL)
2. Update DbContext and connection strings
3. Create Dockerfiles for containerization
4. Setup docker-compose.yml for orchestration
5. Test locally with `docker-compose up`
6. Deploy to Linux servers/cloud

**Linux Deployment Ready** ✅
