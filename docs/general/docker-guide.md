# Docker & Containers - Complete Development Guide
## For .NET Core & PostgreSQL Migration on Windows

---

## Quick Reference: Daily Docker Commands

```bash
# Container Management
docker ps                                   # List running containers
docker ps -a                                # List all containers
docker run -d -p 5000:80 myapp              # Run container in background with port mapping
docker stop container_id                    # Stop container
docker start container_id                   # Start stopped container
docker restart container_id                 # Restart container
docker rm container_id                      # Remove container
docker logs -f container_id                 # Follow container logs
docker exec -it container_id bash           # Access container shell

# Image Management
docker images                               # List all images
docker build -t myapp:latest .              # Build image from Dockerfile
docker pull image:tag                       # Download image from registry
docker rmi image_id                         # Remove image
docker tag source:tag target:tag            # Tag image

# Docker Compose
docker-compose up                           # Start all services
docker-compose up -d                        # Start in background
docker-compose down                         # Stop and remove containers
docker-compose logs -f                      # Follow logs
docker-compose ps                           # List containers
docker-compose restart service_name         # Restart specific service
docker-compose exec service_name bash       # Access service container

# Cleanup
docker system prune                         # Remove unused containers, networks, images
docker system prune -a                      # Remove all unused images
docker volume prune                         # Remove unused volumes

# Inspection
docker inspect container_id                 # Detailed container info
docker stats                                # Real-time resource usage
docker network ls                           # List networks
docker volume ls                            # List volumes
```

---

## Understanding Docker & Containers

### What is Docker?

**Docker** is a platform for developing, shipping, and running applications in containers. Think of it as a lightweight virtualization technology that packages your application and all its dependencies together.

**Key Concepts:**

**Container**
- A running instance of an image
- Isolated environment with its own filesystem, network, and processes
- Lightweight (shares host OS kernel)
- Starts in seconds, not minutes like VMs
- Example: Your .NET Core app running in isolation with all its dependencies

**Image**
- A blueprint/template for creating containers
- Contains application code, runtime, libraries, and dependencies
- Immutable (read-only)
- Built in layers (each instruction in Dockerfile = layer)
- Example: `mcr.microsoft.com/dotnet/aspnet:8.0` - Microsoft's .NET runtime image

**Dockerfile**
- Text file with instructions to build an image
- Defines: base image, copy files, install dependencies, set startup command
- Version-controlled with your code
- Example: Instructions to build your .NET Core application image

**Docker Hub / Container Registry**
- Repository for Docker images (like GitHub for code)
- Public images: PostgreSQL, .NET, Nginx, etc.
- Private registries for your custom images
- Microsoft Container Registry (mcr.microsoft.com) for .NET images

**Volume**
- Persistent data storage outside container filesystem
- Survives container deletion
- Shared between host and container
- Example: PostgreSQL database files, application logs

**Network**
- Virtual network for container communication
- Containers can talk to each other by service name
- Isolated from host by default
- Example: Your .NET app container connecting to PostgreSQL container

### Why Use Docker for .NET Core Migration?

**Benefits:**
1. **Consistency** - Same environment on dev, staging, production
2. **Isolation** - Each service runs independently
3. **Portability** - Run anywhere Docker is installed (Windows, Linux, Cloud)
4. **Easy Setup** - New developer can start in minutes with `docker-compose up`
5. **Microservices** - Perfect for breaking monolith into smaller services
6. **CI/CD** - Easy integration with deployment pipelines
7. **Resource Efficient** - Lighter than VMs, better resource utilization

### Docker vs Virtual Machines

| Feature | Docker Container | Virtual Machine |
|---------|-----------------|-----------------|
| Size | Megabytes | Gigabytes |
| Startup | Seconds | Minutes |
| Performance | Near-native | Overhead from hypervisor |
| Isolation | Process-level | Complete OS isolation |
| Resource Usage | Shares host kernel | Full OS per VM |
| Use Case | Microservices, dev environments | Complete OS isolation needed |

### Container Lifecycle

```
Image (Blueprint)
    ↓ docker run
Container (Created)
    ↓ docker start
Container (Running) ←→ docker stop → Container (Stopped)
    ↓ docker rm
Deleted
```

---

## Docker Desktop Setup on Windows

### Prerequisites
- **Windows 10/11 Pro, Enterprise, or Education** (64-bit)
- **Virtualization enabled** in BIOS
- **WSL 2** (Windows Subsystem for Linux) - Docker Desktop will help install if missing
- **Minimum**: 4GB RAM (8GB+ recommended)
- **Administrator access**

### Step 1: Install Docker Desktop

1. **Download Docker Desktop**
   - Visit: https://www.docker.com/products/docker-desktop
   - Download Windows version
   - File size: ~500MB

2. **Run Installer**
   ```
   - Double-click Docker Desktop Installer.exe
   - Check "Use WSL 2 instead of Hyper-V" (recommended)
   - Click "Ok" to proceed
   - Wait for installation (5-10 minutes)
   - Click "Close and restart" when prompted
   ```

3. **First Launch**
   ```
   - Open Docker Desktop from Start Menu
   - Accept service agreement
   - Skip tutorial (optional)
   - Wait for "Docker Desktop is running" status
   ```

4. **Verify Installation**
   ```powershell
   # Open PowerShell or Command Prompt
   docker --version
   # Output: Docker version 24.0.x, build xxxxx
   
   docker-compose --version
   # Output: Docker Compose version v2.x.x
   
   docker run hello-world
   # Should download and run test container
   ```

### Step 2: Configure Docker Desktop

1. **Open Settings** (Gear icon in Docker Desktop)

2. **General Settings**
   ```
   ✓ Use the WSL 2 based engine
   ✓ Start Docker Desktop when you log in
   ✓ Send usage statistics (optional)
   ```

3. **Resources → Advanced**
   ```
   CPUs: 4 (or half of your total)
   Memory: 8 GB (for development with multiple containers)
   Swap: 2 GB
   Disk image size: 64 GB (can adjust based on available space)
   ```

4. **Resources → File Sharing**
   - Ensure your project directories are accessible
   - Usually: C:\Users\YourName\projects

5. **Docker Engine** (optional - leave default)
   - Advanced Docker daemon configuration (JSON)

### Step 3: Verify WSL 2 Integration

```powershell
# Check WSL version
wsl --list --verbose
# Should show Docker-desktop and Docker-desktop-data

# Set default WSL version to 2 (if needed)
wsl --set-default-version 2

# Update WSL kernel (if needed)
wsl --update
```

### Common Installation Issues

**Issue 1: "WSL 2 installation is incomplete"**
```powershell
Solution:
1. Open PowerShell as Administrator
2. Run: wsl --install
3. Restart computer
4. Open PowerShell again: wsl --set-default-version 2
5. Restart Docker Desktop
```

**Issue 2: "Virtualization is not enabled"**
```
Solution:
1. Restart computer
2. Enter BIOS (usually F2, F10, Delete during boot)
3. Find "Virtualization Technology" or "Intel VT-x" / "AMD-V"
4. Enable it
5. Save and exit BIOS
```

**Issue 3: "Docker Desktop failed to start"**
```powershell
Solution:
1. Close Docker Desktop completely
2. Open PowerShell as Administrator
3. Run: net stop com.docker.service
4. Run: net start com.docker.service
5. Start Docker Desktop again
```

**Issue 4: "An error occurred mounting one of your file shares"**
```
Solution:
1. Docker Desktop Settings → Resources → File Sharing
2. Remove all shared paths
3. Click "Apply & Restart"
4. Add paths back one by one
```

---

## Docker Images for .NET Core & PostgreSQL Migration

### Latest Recommended Images (2025)

#### .NET Images

**1. Runtime Image (Production)**
```dockerfile
mcr.microsoft.com/dotnet/aspnet:8.0
```
- **Use for**: Running compiled .NET applications
- **Size**: ~200 MB
- **Contains**: ASP.NET Core runtime only
- **When**: Production deployments, final stage in multi-stage builds

**2. SDK Image (Development/Build)**
```dockerfile
mcr.microsoft.com/dotnet/sdk:8.0
```
- **Use for**: Building .NET applications, development
- **Size**: ~700 MB
- **Contains**: .NET SDK, compilers, build tools
- **When**: Building applications, development containers

**3. Runtime Deps (Minimal)**
```dockerfile
mcr.microsoft.com/dotnet/runtime-deps:8.0
```
- **Use for**: Self-contained deployments
- **Size**: ~120 MB
- **Contains**: Only native dependencies
- **When**: Smallest possible image with self-contained apps

#### PostgreSQL Images

**1. Official PostgreSQL (Recommended)**
```dockerfile
postgres:16-alpine
```
- **Latest stable version**: 16.x
- **alpine**: Smaller image size (~240 MB vs ~410 MB)
- **Use for**: All environments
- **Note**: Alpine is production-ready and widely used

**2. Full PostgreSQL Image**
```dockerfile
postgres:16
```
- **Size**: ~410 MB
- **Based on**: Debian
- **Use for**: If you need additional tools not in Alpine

**3. Specific Version Pinning**
```dockerfile
postgres:16.1-alpine
```
- **Use for**: Production (lock to specific version)
- **Avoid**: Using `latest` tag in production

### Image Naming Convention

```
registry/repository:tag

Examples:
mcr.microsoft.com/dotnet/aspnet:8.0
└─registry─┘  └─repository─┘ └tag┘

postgres:16-alpine
└─repository─┘└──tag──┘
```

### Pulling Images

```bash
# Pull latest .NET 8 SDK
docker pull mcr.microsoft.com/dotnet/sdk:8.0

# Pull latest .NET 8 Runtime
docker pull mcr.microsoft.com/dotnet/aspnet:8.0

# Pull PostgreSQL 16 Alpine
docker pull postgres:16-alpine

# Verify downloaded images
docker images
```

---

## Complete Setup Guide: .NET Core + PostgreSQL Development Environment

### Project Structure

```
MyProject/
├── src/
│   ├── MyApp/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Program.cs
│   │   ├── MyApp.csproj
│   │   └── appsettings.json
│   └── MyApp.sln
├── docker/
│   ├── Dockerfile
│   └── .dockerignore
├── docker-compose.yml
├── .env
└── README.md
```

### Step 1: Create Dockerfile for .NET Application

**File: `docker/Dockerfile`**

```dockerfile
# Multi-stage build for optimal image size

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (cached layer)
COPY ["MyApp/MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"

# Copy remaining source code
COPY . .
WORKDIR "/src/MyApp"

# Build application
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Set environment
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**Key Points:**
- **Multi-stage build**: Reduces final image size (build artifacts not included)
- **Layer caching**: Restore dependencies first (changes less often)
- **Non-root user**: Security best practice
- **Small final image**: Only runtime + published app (~200-250 MB)

### Step 2: Create .dockerignore

**File: `docker/.dockerignore`**

```
# Build artifacts
bin/
obj/
out/

# User-specific files
*.user
*.suo
*.userosscache
*.sln.docstates

# Visual Studio
.vs/
.vscode/

# Test results
TestResults/
[Tt]est[Rr]esult*/

# NuGet
packages/
*.nupkg
*.snupkg

# Docker
Dockerfile*
docker-compose*
.dockerignore

# Git
.git/
.gitignore
.gitattributes

# Documentation
README.md
*.md

# OS files
.DS_Store
Thumbs.db

# Logs
logs/
*.log
```

**Purpose**: Exclude unnecessary files from Docker build context (faster builds)

### Step 3: Create docker-compose.yml

**File: `docker-compose.yml`**

```yaml
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: myapp_postgres
    restart: unless-stopped
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      POSTGRES_DB: ${POSTGRES_DB:-myappdb}
      PGDATA: /var/lib/postgresql/data/pgdata
    volumes:
      # Persistent data storage
      - postgres_data:/var/lib/postgresql/data
      # Initialization scripts (optional)
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - myapp_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # .NET Core Application
  myapp:
    build:
      context: ./src
      dockerfile: ../docker/Dockerfile
    container_name: myapp_web
    restart: unless-stopped
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${POSTGRES_DB:-myappdb};Username=${POSTGRES_USER:-postgres};Password=${POSTGRES_PASSWORD:-postgres}
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - myapp_network
    volumes:
      # Hot reload for development (optional)
      - ./src:/app
      # Logs
      - ./logs:/app/logs

  # pgAdmin (Database Management Tool - Optional)
  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: myapp_pgadmin
    restart: unless-stopped
    ports:
      - "5050:80"
    environment:
      PGADMIN_DEFAULT_EMAIL: ${PGADMIN_EMAIL:-admin@admin.com}
      PGADMIN_DEFAULT_PASSWORD: ${PGADMIN_PASSWORD:-admin}
      PGADMIN_CONFIG_SERVER_MODE: 'False'
    volumes:
      - pgadmin_data:/var/lib/pgadmin
    networks:
      - myapp_network
    depends_on:
      - postgres

networks:
  myapp_network:
    driver: bridge

volumes:
  postgres_data:
    driver: local
  pgadmin_data:
    driver: local
```

**Key Features:**
- **Health checks**: App waits for database to be ready
- **Named volumes**: Data persists between container restarts
- **Custom network**: Services communicate by name
- **Environment variables**: Configurable via .env file
- **pgAdmin**: Web-based database management (optional)

### Step 4: Create Environment File

**File: `.env`**

```bash
# PostgreSQL Configuration
POSTGRES_USER=devuser
POSTGRES_PASSWORD=DevPassword123!
POSTGRES_DB=myappdb

# pgAdmin Configuration
PGADMIN_EMAIL=admin@myapp.com
PGADMIN_PASSWORD=AdminPassword123!

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
```

**Security Note**: Add `.env` to `.gitignore` - never commit passwords to repository

### Step 5: Update appsettings.json

**File: `src/MyApp/appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=myappdb;Username=postgres;Password=postgres"
  }
}
```

**Note**: Connection string will be overridden by environment variable in container

### Step 6: Update .NET Project for PostgreSQL

**Install Npgsql Entity Framework Core Provider**

```powershell
# Navigate to project directory
cd src/MyApp

# Install PostgreSQL provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Install EF Core tools (if not already)
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**Update Program.cs or Startup.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null
        )
    )
);

// Add Swagger for API documentation (optional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run migrations automatically (optional - for development only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Apply pending migrations
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Step 7: Create Database Context

**File: `src/MyApp/Data/ApplicationDbContext.cs`**

```csharp
using Microsoft.EntityFrameworkCore;

namespace MyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your DbSets here
        public DbSet<User> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entities
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // PostgreSQL specific: use JSONB for JSON columns
                entity.Property(e => e.Metadata).HasColumnType("jsonb");
            });
        }
    }
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Metadata { get; set; } // JSON field
    }
}
```

### Step 8: Create Initial Migration

```powershell
# Navigate to project directory
cd src/MyApp

# Create initial migration
dotnet ef migrations add InitialCreate

# This creates Migrations folder with migration files
```

### Step 9: Start the Development Environment

```powershell
# Navigate to project root (where docker-compose.yml is)
cd C:\path\to\MyProject

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Check running containers
docker-compose ps

# Expected output:
# NAME              STATUS              PORTS
# myapp_postgres    Up (healthy)        0.0.0.0:5432->5432/tcp
# myapp_web         Up                  0.0.0.0:5000->80/tcp
# myapp_pgadmin     Up                  0.0.0.0:5050->80/tcp
```

### Step 10: Verify Setup

**1. Check Application**
```
Open browser: http://localhost:5000
Swagger UI: http://localhost:5000/swagger
```

**2. Check Database (pgAdmin)**
```
Open browser: http://localhost:5050
Login: admin@myapp.com / AdminPassword123!

Add server:
- Name: Local PostgreSQL
- Host: postgres (service name, not localhost!)
- Port: 5432
- Username: devuser
- Password: DevPassword123!
```

**3. Test Database Connection**
```powershell
# Connect to PostgreSQL container
docker exec -it myapp_postgres psql -U devuser -d myappdb

# Run test query
SELECT version();

# List tables
\dt

# Exit
\q
```

### Step 11: Development Workflow

**Making Code Changes**
```powershell
# Code changes are detected automatically if using volume mount
# If not using volume mount, rebuild:
docker-compose up -d --build myapp

# View application logs
docker-compose logs -f myapp

# Restart specific service
docker-compose restart myapp
```

**Running Migrations**
```powershell
# Option 1: Run migration inside container
docker-compose exec myapp dotnet ef database update

# Option 2: Run migration from host (if .NET SDK installed)
cd src/MyApp
dotnet ef database update
```

**Accessing Database**
```powershell
# Using psql in container
docker exec -it myapp_postgres psql -U devuser -d myappdb

# Using pgAdmin
# Open http://localhost:5050 in browser
```

**Viewing Logs**
```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f myapp
docker-compose logs -f postgres

# Last 100 lines
docker-compose logs --tail=100 myapp
```

**Stopping and Cleaning Up**
```powershell
# Stop all services (keeps data)
docker-compose down

# Stop and remove volumes (deletes data!)
docker-compose down -v

# Stop and remove images
docker-compose down --rmi all
```

---

## Common Issues and Solutions

### Issue 1: "Port is already in use"

**Error:**
```
Error starting userland proxy: listen tcp 0.0.0.0:5432: bind: address already in use
```

**Cause**: Another application is using the port (e.g., local PostgreSQL installation)

**Solutions:**

**Option A: Stop conflicting service**
```powershell
# Find process using port
netstat -ano | findstr :5432

# Kill process (replace PID with actual process ID)
taskkill /PID 1234 /F
```

**Option B: Change port in docker-compose.yml**
```yaml
postgres:
  ports:
    - "5433:5432"  # Use 5433 on host, 5432 in container
```

**Option C: Stop local PostgreSQL service**
```powershell
# Windows Services
services.msc
# Find PostgreSQL, right-click, Stop
```

### Issue 2: "No connection could be made"

**Error:**
```
Npgsql.NpgsqlException: Failed to connect to localhost:5432
```

**Cause**: Application trying to connect to localhost instead of container name

**Solution:**

**Connection string must use service name:**
```
❌ Wrong: Host=localhost;Port=5432;...
✓ Correct: Host=postgres;Port=5432;...
```

**In docker-compose.yml environment:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=myappdb;...
```

### Issue 3: "Database does not exist"

**Error:**
```
Npgsql.PostgresException: database "myappdb" does not exist
```

**Cause**: Database not created or wrong database name

**Solution:**

**Check container logs:**
```powershell
docker-compose logs postgres
```

**Recreate database:**
```powershell
# Connect to PostgreSQL
docker exec -it myapp_postgres psql -U postgres

# Create database
CREATE DATABASE myappdb;
GRANT ALL PRIVILEGES ON DATABASE myappdb TO devuser;
\q

# Or restart with fresh volumes
docker-compose down -v
docker-compose up -d
```

### Issue 4: "denied: permission denied"

**Error:**
```
error creating container: Error response from daemon: 
pull access denied for myapp, repository does not exist
```

**Cause**: Trying to pull image that doesn't exist in registry

**Solution:**

**Build image locally first:**
```powershell
# Build image
docker-compose build

# Then run
docker-compose up -d
```

### Issue 5: "Cannot start service: driver failed"

**Error:**
```
Error response from daemon: driver failed programming external connectivity
```

**Cause**: Docker networking issue or Windows Firewall

**Solutions:**

**Restart Docker:**
```powershell
# Stop Docker Desktop completely
# Start Docker Desktop
```

**Restart network:**
```powershell
docker network prune
docker-compose down
docker-compose up -d
```

**Windows Firewall:**
```
1. Windows Security → Firewall & network protection
2. Allow an app through firewall
3. Ensure Docker Desktop is allowed
```

### Issue 6: "Waiting for postgres to be ready"

**Error:**
Application starts before database is ready

**Solution:**

**Add health check and depends_on:**
```yaml
postgres:
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5

myapp:
  depends_on:
    postgres:
      condition: service_healthy
```

### Issue 7: "Volume mount failed"

**Error:**
```
Error response from daemon: error while creating mount source path
```

**Cause**: Windows path not accessible or wrong format

**Solution:**

**Use correct path format:**
```yaml
# ❌ Wrong
volumes:
  - C:\Users\Name\project:/app

# ✓ Correct (use forward slashes)
volumes:
  - C:/Users/Name/project:/app

# ✓ Or use relative paths
volumes:
  - ./src:/app
```

**Enable file sharing in Docker Desktop:**
```
Settings → Resources → File Sharing
Add C:\ drive
Apply & Restart
```

### Issue 8: "Migrations not applying"

**Error:**
Migrations exist but database tables not created

**Solution:**

**Apply migrations manually:**
```powershell
# From host (if .NET SDK installed)
cd src/MyApp
dotnet ef database update

# From container
docker-compose exec myapp dotnet ef database update

# Check migration status
docker-compose exec myapp dotnet ef migrations list
```

**Verify connection string:**
```csharp
// Log connection string on startup (remove in production!)
Console.WriteLine($"Connection: {builder.Configuration.GetConnectionString("DefaultConnection")}");
```

### Issue 9: "Container keeps restarting"

**Symptoms:**
```powershell
docker-compose ps
# Shows: myapp_web   Restarting
```

**Solution:**

**Check logs:**
```powershell
docker-compose logs myapp

# Common causes:
# - Application crash on startup
# - Missing environment variable
# - Invalid connection string
# - Port already in use inside container
```

**Debug interactively:**
```powershell
# Override entrypoint to bash
docker-compose run --entrypoint bash myapp

# Inside container, try running app manually
dotnet MyApp.dll
```

### Issue 10: "Image too large / Build too slow"

**Solution:**

**Use .dockerignore:**
```
# Exclude build artifacts and unnecessary files
bin/
obj/
.vs/
*.user
```

**Multi-stage build:**
```dockerfile
# Build stage (large)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build commands ...

# Runtime stage (small)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
COPY --from=build /app/publish .
```

**Layer caching:**
```dockerfile
# Copy and restore first (changes less often)
COPY ["MyApp.csproj", "MyApp/"]
RUN dotnet restore

# Copy source code last (changes frequently)
COPY . .
```

---

## Docker Compose Advanced Patterns

### Multiple Environments

**docker-compose.dev.yml** (Development)
```yaml
version: '3.8'

services:
  myapp:
    build:
      target: development
    volumes:
      - ./src:/app  # Hot reload
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

**docker-compose.prod.yml** (Production)
```yaml
version: '3.8'

services:
  myapp:
    build:
      target: final
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

**Usage:**
```powershell
# Development
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# Production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Multiple Projects

**docker-compose.yml**
```yaml
version: '3.8'

services:
  api:
    build: ./ApiProject
    ports:
      - "5000:80"
    depends_on:
      - postgres

  web:
    build: ./WebProject
    ports:
      - "5001:80"
    depends_on:
      - api

  postgres:
    image: postgres:16-alpine
    # ... config ...
```

### Scaling Services

```powershell
# Run 3 instances of API
docker-compose up -d --scale api=3

# Note: Remove port mapping for scaled services
# Use nginx/load balancer to distribute traffic
```

---

## Best Practices

### Security

1. **Never use root user in containers**
   ```dockerfile
   RUN adduser --disabled-password appuser
   USER appuser
   ```

2. **Use secrets for sensitive data (not environment variables)**
   ```yaml
   secrets:
     db_password:
       file: ./secrets/db_password.txt
   ```

3. **Pin image versions**
   ```dockerfile
   FROM postgres:16.1-alpine  # Not just :16 or :latest
   ```

4. **Scan images for vulnerabilities**
   ```powershell
   docker scan myapp:latest
   ```

### Performance

1. **Use layer caching effectively**
   - Copy dependency files first
   - Copy source code last

2. **Use .dockerignore**
   - Exclude unnecessary files
   - Faster builds, smaller context

3. **Multi-stage builds**
   - Separate build and runtime
   - Smaller final images

4. **Use alpine images when possible**
   - Smaller size
   - Faster downloads
   - Less attack surface

### Development

1. **Use volumes for code**
   ```yaml
   volumes:
     - ./src:/app  # Changes reflected immediately
   ```

2. **Name your containers**
   ```yaml
   container_name: myapp_web  # Easier to reference
   ```

3. **Use health checks**
   ```yaml
   healthcheck:
     test: ["CMD", "curl", "-f", "http://localhost/health"]
   ```

4. **Log to stdout/stderr**
   ```csharp
   // Not to files - Docker captures stdout
   Console.WriteLine("Log message");
   ```

### Production

1. **Use restart policies**
   ```yaml
   restart: unless-stopped  # or 'always'
   ```

2. **Set resource limits**
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '2'
         memory: 2G
   ```

3. **Use environment-specific configs**
   ```yaml
   env_file:
     - .env.production
   ```

4. **Regular backups**
   ```powershell
   # Backup PostgreSQL
   docker exec myapp_postgres pg_dump -U postgres myappdb > backup.sql
   ```

---

## Useful Commands Cheat Sheet

### Debugging

```powershell
# View real-time logs
docker-compose logs -f

# Execute commands in running container
docker-compose exec myapp bash

# Check container resource usage
docker stats

# Inspect container details
docker inspect myapp_web

# List networks
docker network ls

# Inspect network
docker network inspect myapp_network

# List volumes
docker volume ls

# Inspect volume
docker volume inspect myapp_postgres_data
```

### Cleanup

```powershell
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Remove unused networks
docker network prune

# Remove everything unused
docker system prune -a --volumes

# Check disk usage
docker system df
```

### Building

```powershell
# Build without cache
docker-compose build --no-cache

# Build specific service
docker-compose build myapp

# Pull latest images
docker-compose pull
```

---

## Next Steps

1. **Learn Docker Networking** - Understand how containers communicate
2. **Explore Docker Volumes** - Data persistence and backup strategies
3. **CI/CD Integration** - Automate build and deployment
4. **Kubernetes** - Container orchestration for production scale
5. **Docker Security** - Scanning, secrets management, best practices

---

## Additional Resources

**Official Documentation:**
- Docker Docs: https://docs.docker.com
- .NET on Docker: https://learn.microsoft.com/en-us/dotnet/core/docker/
- PostgreSQL Docker: https://hub.docker.com/_/postgres

**Tools:**
- Docker Desktop: https://www.docker.com/products/docker-desktop
- Docker Compose: https://docs.docker.com/compose/
- pgAdmin: https://www.pgadmin.org/

**Learning:**
- Docker Getting Started: https://docs.docker.com/get-started/
- Play with Docker: https://labs.play-with-docker.com/