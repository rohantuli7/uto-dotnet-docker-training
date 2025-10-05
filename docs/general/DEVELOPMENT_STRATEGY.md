# Development Strategy Guide

## Overview

This guide outlines the development strategy for the UTO Docker .NET Dashboard project, designed specifically for developers new to Docker and CI/CD practices. The project uses a modern containerized approach with .NET 8.0, PostgreSQL, and Docker Compose.

## Table of Contents

1. [Development Philosophy](#development-philosophy)
2. [Docker Development Strategy](#docker-development-strategy)
3. [Branching Strategy](#branching-strategy)
4. [CI/CD Pipeline](#cicd-pipeline)
5. [Development Workflow](#development-workflow)
6. [Environment Management](#environment-management)
7. [Testing Strategy](#testing-strategy)
8. [Deployment Strategy](#deployment-strategy)
9. [FAQ](#faq)

## Development Philosophy

### Core Principles

1. **Container-First Development**: All development happens within Docker containers to ensure environment consistency
2. **Feature Branch Workflow**: Each feature/bug fix is developed in isolated branches
3. **Automated Testing**: Every commit triggers automated tests and builds
4. **Infrastructure as Code**: All environments are defined in code (Docker Compose files)
5. **Database Schema Management**: Database changes are version-controlled and automated

### Why Docker for Development?

- **Environment Consistency**: "It works on my machine" becomes "It works in our container"
- **Simplified Setup**: New developers can start coding in minutes, not hours
- **Isolation**: Each service runs in its own container, preventing conflicts
- **Production Parity**: Development environment matches production exactly

## Docker Development Strategy

### Our Development Strategy: Build-on-Change Docker Development

**Current Project Setup:**
The project uses **Production-Style Docker Development** - code is built into containers during development. This approach prioritizes production parity over development speed.

#### How It Works:
- Code lives on your host machine (edit with VS Code, etc.)
- Code is copied into containers during Docker build process
- Multi-stage Dockerfiles compile and package the applications
- Database runs in containers with persistent volumes
- Git operations happen on host machine
- **Requires container rebuild for each code change**

#### Current Architecture:
```
Host Machine (Your Computer)
├── 📁 uto_docker_dotnet/          # Git repository here
│   ├── 📁 Backend/                # Edit with VS Code
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Dockerfile             # Multi-stage production build
│   │   └── Program.cs
│   ├── 📁 Frontend/               # Edit with VS Code
│   │   ├── Pages/
│   │   └── Dockerfile             # Multi-stage production build
│   └── docker-compose.yml
│
Docker Containers (Production-like)
├── 📦 Backend Container           # Code copied during build
│   ├── .NET Runtime              # Optimized runtime image
│   ├── Published App             # Release build
│   └── Port 8080
├── 📦 Frontend Container          # Code copied during build
│   ├── .NET Runtime              # Optimized runtime image
│   ├── Published App             # Release build
│   └── Port 5000
└── 📦 PostgreSQL Container        # Persistent volume: postgres_data
    └── Port 5432
```

### Multi-Stage Dockerfile Strategy

The current Docker setup uses production-style multi-stage builds for both frontend and backend:

```dockerfile
# Backend/Dockerfile & Frontend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.dll"]
```

**Key Points:**
- Code is copied into containers during build
- No volume mounts for source code
- Optimized runtime images (smaller, production-like)
- Requires full rebuild for any code changes

## Branching Strategy

### Git Flow for CI/CD

We use a simplified Git Flow optimized for continuous integration:

```
main
├── develop
│   ├── feature/user-authentication
│   ├── feature/dashboard-filters
│   └── bugfix/api-error-handling
└── hotfix/critical-security-patch
```

### Branch Types

#### 1. `main` Branch
- **Purpose**: Production-ready code
- **Protection**:
  - Direct pushes disabled
  - Requires pull request reviews
  - All CI checks must pass
  - Auto-deployment to production

#### 2. `develop` Branch
- **Purpose**: Integration branch for features
- **CI Behavior**:
  - Runs full test suite
  - Builds Docker images
  - Deploys to staging environment
  - Runs integration tests

#### 3. `feature/*` Branches
- **Purpose**: New features and enhancements
- **Naming**: `feature/short-description`
- **CI Behavior**:
  - Runs unit tests
  - Builds Docker images
  - Deploys to feature environment (optional)

#### 4. `bugfix/*` Branches
- **Purpose**: Non-critical bug fixes
- **Naming**: `bugfix/issue-description`
- **CI Behavior**: Same as feature branches

#### 5. `hotfix/*` Branches
- **Purpose**: Critical production fixes
- **Naming**: `hotfix/critical-issue`
- **CI Behavior**:
  - Fast-tracked testing
  - Direct merge to main after review
  - Immediate production deployment

## CI/CD Pipeline

### Pipeline Overview

Our CI/CD pipeline uses GitHub Actions (or similar) with Docker-based builds:

```yaml
# .github/workflows/ci-cd.yml (example structure)
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build and Test
        run: |
          docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit

  build:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - name: Build Production Images
        run: |
          docker build -t dashboard-frontend:${{ github.sha }} ./Frontend
          docker build -t dashboard-backend:${{ github.sha }} ./Backend

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to Production
        run: |
          # Deployment commands
```

### Pipeline Stages

#### 1. **Code Quality** (All Branches)
```bash
# Lint check
docker run --rm -v $(pwd):/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  sh -c "dotnet format --verify-no-changes"

# Security scan
docker run --rm -v $(pwd):/src codeql/codeql-runner
```

#### 2. **Unit Testing** (All Branches)
```bash
# Backend tests
docker-compose -f docker-compose.test.yml run --rm backend-test

# Frontend tests (if applicable)
docker-compose -f docker-compose.test.yml run --rm frontend-test
```

#### 3. **Integration Testing** (develop, main)
```bash
# Start all services
docker-compose -f docker-compose.test.yml up -d

# Run integration tests
docker-compose -f docker-compose.test.yml run --rm integration-tests

# Cleanup
docker-compose -f docker-compose.test.yml down -v
```

#### 4. **Build & Push Images** (develop, main)
```bash
# Build with commit SHA tag
docker build -t registry/dashboard-backend:${COMMIT_SHA} ./Backend
docker build -t registry/dashboard-frontend:${COMMIT_SHA} ./Frontend

# Push to registry
docker push registry/dashboard-backend:${COMMIT_SHA}
docker push registry/dashboard-frontend:${COMMIT_SHA}
```

#### 5. **Deployment**
- **develop branch** → Staging environment
- **main branch** → Production environment

### Docker in CI/CD

#### Why Docker in CI/CD?

1. **Consistent Builds**: Same Docker images from development to production
2. **Fast Builds**: Layer caching reduces build times
3. **Parallel Testing**: Multiple test suites in separate containers
4. **Environment Isolation**: Each pipeline run is isolated
5. **Easy Rollbacks**: Tagged images enable quick rollbacks

#### Multi-Stage Docker Builds

```dockerfile
# Backend/Dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DashboardApi.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

# Test stage
FROM build AS test
RUN dotnet test --logger trx --results-directory /app/test-results

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DashboardApi.dll"]
```

#### Docker Compose for Different Environments

**Development** (`docker-compose.yml`):
```yaml
services:
  backend:
    build: ./Backend
    ports: ["8080:8080"]
    volumes: ["./Backend:/app"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

**Testing** (`docker-compose.test.yml`):
```yaml
services:
  backend:
    build:
      context: ./Backend
      target: test
    environment:
      - ASPNETCORE_ENVIRONMENT=Testing

  integration-tests:
    build: ./Tests
    depends_on: [backend, postgres]
```

**Production** (`docker-compose.prod.yml`):
```yaml
services:
  backend:
    image: registry/dashboard-backend:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    deploy:
      replicas: 3
```

## Development Workflow

### Daily Development Workflow

#### Step 1: Initial Setup (One-time per developer)
```bash
# Clone the repository to your computer
git clone <repository-url>
cd uto_docker_dotnet

# First-time build and start (takes 2-3 minutes)
docker-compose up --build -d

# Verify everything is working
curl http://localhost:8080/api/dashboard
```

#### Step 2: Daily Development Session
```bash
# Pull latest changes (on host machine)
git checkout develop
git pull origin develop

# Create feature branch (on host machine)
git checkout -b feature/new-dashboard-widget

# Ensure containers are running
docker-compose ps

# View logs for debugging
docker-compose logs -f backend
```

#### Step 3: Code Development (Build-on-Change)
```bash
# Open VS Code (on host machine)
code .

# Edit any file in Backend/ or Frontend/
# Example: Backend/Controllers/DashboardController.cs

# IMPORTANT: After making changes, rebuild containers:
docker-compose up --build -d

# This process takes 1-2 minutes as it:
# 1. Rebuilds the Docker image
# 2. Compiles your .NET code
# 3. Creates optimized runtime container
# 4. Restarts the service
```

#### Step 4: Testing Your Changes
```bash
# After rebuild completes, test your changes
curl http://localhost:8080/api/dashboard

# Test frontend changes in browser
open http://localhost:5000

# Run tests inside container (optional)
docker-compose exec backend dotnet test

# View database changes
docker exec dashboard-postgres psql -U postgres -d dashboarddb \
  -c "SELECT * FROM \"DashboardItems\";"
```

#### Step 5: Development Cycle
```bash
# For each code change, repeat:
# 1. Edit code in VS Code
# 2. Save files
# 3. Rebuild containers: docker-compose up --build -d
# 4. Wait 1-2 minutes
# 5. Test changes
# 6. Repeat...
```

#### Step 6: Git Operations (On Host Machine)
```bash
# All git operations happen on your computer (not in containers)
git status                    # See your changes
git add .                     # Stage changes
git commit -m "Add new feature"  # Commit
git push origin feature/new-dashboard-widget  # Push to GitHub
```

#### Step 7: Create Pull Request
- Open GitHub and create PR
- CI automatically runs tests in clean Docker environment
- Team reviews your code
- Merge after approval

### Development Characteristics

#### What You DON'T Need to Do
❌ **Don't enter containers to edit code**
❌ **Don't install .NET SDK locally**
❌ **Don't run git commands inside containers**

#### What You DO (Build-on-Change Workflow)
✅ **Edit code in VS Code on your computer**
✅ **Rebuild containers after each change** (`docker-compose up --build -d`)
✅ **Wait 1-2 minutes for rebuild to complete**
✅ **Use git normally on your computer**
✅ **Let Docker handle the .NET environment**

### Trade-offs of Current Setup

#### Advantages:
- ✅ **Production Parity**: Exact same build process as production
- ✅ **No Local Dependencies**: Zero .NET installation required
- ✅ **Team Consistency**: Everyone uses identical environment
- ✅ **Docker Expertise**: Learn production Docker patterns
- ✅ **Optimized Images**: Smaller, secure runtime containers

#### Disadvantages:
- ⏱️ **Slower Development**: 1-2 minutes per change
- 🔄 **Rebuild Required**: Every code change needs container rebuild
- 💻 **Resource Intensive**: Full compilation each time
- 🐌 **Feedback Loop**: Longer time between code and testing

### Complete Development Example

Here's a complete example of developing a new feature:

#### Scenario: Add a new API endpoint for user statistics

```bash
# 1. Start from clean state
git checkout develop
git pull origin develop
git checkout -b feature/user-statistics

# 2. Ensure development environment is running
docker-compose up -d

# 3. Check container status
docker-compose ps
```

**Now in VS Code (on your computer):**

```csharp
// 4. Edit Backend/Controllers/DashboardController.cs
// Add this new method:

[HttpGet("statistics")]
public async Task<ActionResult<object>> GetStatistics()
{
    var totalItems = await _context.DashboardItems.CountAsync();
    var recentItems = await _context.DashboardItems
        .Where(x => x.CreatedAt > DateTime.UtcNow.AddDays(-7))
        .CountAsync();

    return Ok(new {
        TotalItems = totalItems,
        RecentItems = recentItems
    });
}
```

**Save the file and rebuild containers:**
```bash
# 5. IMPORTANT: Rebuild containers to apply changes
docker-compose up --build -d

# This takes 1-2 minutes and will:
# - Rebuild backend Docker image
# - Compile your new code
# - Start the updated container
```

**Test after rebuild completes:**
```bash
# 6. Test your new endpoint (after rebuild finishes)
curl http://localhost:8080/api/dashboard/statistics

# Response: {"totalItems":5,"recentItems":5}
```

**Continue development:**
```bash
# 7. Add frontend page to display statistics
# Edit Frontend/Pages/Statistics.cshtml (in VS Code)
# Save files

# 8. Rebuild frontend container
docker-compose up --build frontend -d

# 9. Test in browser
open http://localhost:5000/statistics

# 10. Commit your changes
git add .
git commit -m "Add user statistics endpoint and page"
git push origin feature/user-statistics

# 11. Create PR on GitHub
# CI will test your changes in identical Docker environment
```

### Key Points About This Workflow

1. **Code Location**: All code lives on your computer, not in containers
2. **Editing**: Use any IDE (VS Code, Visual Studio, etc.) normally
3. **Git**: Normal git workflow on your computer
4. **Build-on-Change**: Save file → Rebuild container → Test changes
5. **Rebuild Required**: Must rebuild containers for every code change
6. **Consistency**: Everyone has identical environment regardless of OS
7. **Production Parity**: Same build process as production deployment

### Code Review Process

#### Pull Request Checklist
- [ ] All CI checks pass
- [ ] Code follows project conventions
- [ ] Tests added for new functionality
- [ ] Documentation updated if needed
- [ ] No sensitive data committed
- [ ] Database migrations reviewed (if any)

#### Automated Checks
- Docker builds successfully
- All tests pass
- Code coverage maintains threshold
- Security vulnerabilities scan clean
- Performance benchmarks within limits

## Environment Management

### Environment Types

#### 1. **Development (Local)**
- **Purpose**: Individual developer work
- **Database**: Fresh PostgreSQL container
- **Data**: Seed data only
- **Debugging**: Full debugging enabled
- **Hot Reload**: Enabled for rapid development

#### 2. **Feature Environments** (Optional)
- **Purpose**: Testing specific features
- **Lifecycle**: Created on feature branch push, destroyed on merge
- **Database**: Isolated PostgreSQL instance
- **Data**: Seed data + feature-specific test data

#### 3. **Staging**
- **Purpose**: Integration testing
- **Database**: PostgreSQL with production-like data (anonymized)
- **Monitoring**: Full monitoring stack
- **Performance**: Production-like resources

#### 4. **Production**
- **Purpose**: Live application
- **Database**: PostgreSQL with backups and replication
- **Monitoring**: Full observability
- **Security**: Enhanced security measures

### Environment Configuration

Each environment uses different Docker Compose configurations:

```bash
# Development
docker-compose up

# Staging
docker-compose -f docker-compose.yml -f docker-compose.staging.yml up

# Production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up
```

### Database Management Across Environments

#### Development
```bash
# Reset database
docker-compose down -v
docker-compose up -d postgres

# View data
docker exec dashboard-postgres psql -U postgres -d dashboarddb \
  -c "SELECT * FROM \"DashboardItems\";"
```

#### Staging/Production
```bash
# Backup before deployment
docker exec dashboard-postgres pg_dump -U postgres dashboarddb > backup.sql

# Apply migrations
docker-compose exec backend dotnet ef database update

# Verify deployment
curl http://staging.example.com/api/dashboard
```

## Testing Strategy

### Test Pyramid

#### 1. **Unit Tests** (Fast, Many)
```bash
# Backend unit tests
cd Backend
dotnet test

# Run specific test
dotnet test --filter "DashboardControllerTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### 2. **Integration Tests** (Medium, Some)
```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run integration tests
docker-compose -f docker-compose.test.yml run --rm integration-tests

# Test API endpoints
curl http://localhost:8080/api/dashboard
```

#### 3. **End-to-End Tests** (Slow, Few)
```bash
# Start full application
docker-compose up -d

# Run E2E tests (using tools like Playwright)
docker run --rm --network host playwright-tests
```

### Testing in Docker

#### Benefits
- **Isolation**: Each test run is completely isolated
- **Consistency**: Same environment across all developers and CI
- **Speed**: Parallel test execution in containers
- **Cleanup**: Easy environment reset between test runs

#### Test Containers Pattern
```yaml
# docker-compose.test.yml
version: '3.8'
services:
  backend-test:
    build:
      context: ./Backend
      target: test
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres-test;Port=5432;Database=testdb;Username=postgres;Password=postgres123
    depends_on:
      - postgres-test

  postgres-test:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: testdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
    tmpfs:
      - /var/lib/postgresql/data  # In-memory for speed
```

## Deployment Strategy

### Blue-Green Deployment

#### Overview
Maintain two identical production environments (Blue and Green) and switch between them:

```bash
# Deploy to green environment
docker-compose -f docker-compose.prod-green.yml up -d

# Run health checks
curl http://green.example.com/health

# Switch traffic (load balancer configuration)
# If successful, blue becomes the new green
```

#### Benefits
- Zero downtime deployments
- Easy rollback capability
- Full testing in production environment
- Risk mitigation

### Rolling Updates

For gradual updates with multiple replicas:

```yaml
# docker-compose.prod.yml
services:
  backend:
    image: registry/dashboard-backend:${VERSION}
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 30s
        failure_action: rollback
```

### Rollback Strategy

#### Automatic Rollback
```bash
# If health checks fail, automatically rollback
if ! curl -f http://localhost:8080/health; then
  docker-compose -f docker-compose.prod.yml up -d --scale backend=3 \
    registry/dashboard-backend:${PREVIOUS_VERSION}
fi
```

#### Manual Rollback
```bash
# List available versions
docker images registry/dashboard-backend

# Rollback to previous version
export VERSION=previous-commit-sha
docker-compose -f docker-compose.prod.yml up -d
```

### Database Migrations in Production

#### Safe Migration Strategy
1. **Backward Compatible Migrations**: Always ensure new code works with old schema
2. **Gradual Schema Changes**: Add columns first, remove later
3. **Zero-Downtime Migrations**: Use techniques that don't lock tables
4. **Rollback Plan**: Always have a rollback strategy for schema changes

```bash
# Production migration workflow
# 1. Deploy code that works with both old and new schema
docker-compose -f docker-compose.prod.yml up -d

# 2. Run migration
docker-compose -f docker-compose.prod.yml exec backend \
  dotnet ef database update

# 3. Verify application health
curl http://localhost:8080/health

# 4. Deploy code that requires new schema (next release)
```

## FAQ

### General Development Questions

#### Q: Do I need to install .NET SDK locally to develop?

**A: No! This project is set up for pure Docker development.**

**Our Setup: Build-on-Change Docker Development**
- ✅ No local .NET installation required
- ✅ Code editing happens on your computer (VS Code, etc.)
- ✅ .NET SDK runs inside containers during build process
- ⏱️ Requires rebuild for each change (1-2 minutes)
- ✅ Perfect environment and production parity

```bash
# How it works:
# 1. Clone repo to your computer
git clone <repo-url>
cd uto_docker_dotnet

# 2. Start containers (first build takes 2-3 minutes)
docker-compose up --build -d

# 3. Edit code in VS Code on your computer
code Backend/Controllers/DashboardController.cs

# 4. Save file → Rebuild container → Test changes
docker-compose up --build -d  # Takes 1-2 minutes
curl http://localhost:8080/api/dashboard  # Test changes
```

**Alternative: Hybrid Development (Not recommended for this project)**
```bash
# If you really want to install .NET locally:
# 1. Install .NET 8.0 SDK
# 2. Start only database
docker-compose up postgres pgadmin -d

# 3. Run services locally
cd Backend && dotnet run
cd Frontend && dotnet run
```

**Why We Use Build-on-Change Instead of Volume Mounts:**
- **Production Parity**: Exact same build process from dev to production
- **Docker Learning**: Teams learn real-world Docker patterns
- **Consistency**: Identical compiled artifacts across environments
- **Security**: Production-ready container images from day one

#### Q: How do I debug the application?

**A: Multiple debugging options available.**

**Option 1: Docker Logs**
```bash
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f backend

# View logs with timestamps
docker-compose logs -f -t backend
```

**Option 2: Attach Debugger (VS Code)**
```json
// .vscode/launch.json
{
  "name": "Attach to Backend Container",
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickRemoteProcess}",
  "pipeTransport": {
    "pipeProgram": "docker",
    "pipeArgs": ["exec", "-i", "dashboard-backend"],
    "debuggerPath": "/vsdbg/vsdbg"
  }
}
```

**Option 3: Local Development with Docker Database**
```bash
# Start only database
docker-compose up postgres -d

# Debug locally with full IDE support
cd Backend
dotnet run --environment Development
```

#### Q: How do I reset my development environment?

**A: Simple commands to reset everything.**

```bash
# Nuclear option - reset everything
docker-compose down -v --remove-orphans
docker system prune -f
docker-compose up --build -d

# Partial reset - just database
docker-compose down -v
docker-compose up -d

# Reset specific service
docker-compose stop backend
docker-compose rm -f backend
docker-compose up --build backend -d
```

#### Q: How do I add a new database table/model?

**A: Follow these steps for database changes.**

1. **Create the model class**:
```csharp
// Backend/Models/NewModel.cs
public class NewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

2. **Add to DbContext**:
```csharp
// Backend/Data/DashboardContext.cs
public DbSet<NewModel> NewModels { get; set; }
```

3. **Update seed data if needed**:
```csharp
// In DashboardContext.OnModelCreating
modelBuilder.Entity<NewModel>().HasData(
    new NewModel { Id = 1, Name = "Sample", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
);
```

4. **Reset database** (since we use EnsureCreated):
```bash
docker-compose down -v
docker-compose up -d
```

5. **Verify the change**:
```bash
docker exec dashboard-postgres psql -U postgres -d dashboarddb -c "\dt"
```

### CI/CD Questions

#### Q: What happens when I push a commit?

**A: Automatic CI pipeline triggers.**

**For feature branches:**
1. Code quality checks (linting, formatting)
2. Unit tests run in Docker containers
3. Docker images built and tested
4. Security scans performed
5. Results posted to pull request

**For develop branch:**
1. All feature branch checks
2. Integration tests with full environment
3. Images pushed to registry with `develop-latest` tag
4. Automatic deployment to staging environment
5. Smoke tests run against staging

**For main branch:**
1. All develop branch checks
2. Additional production readiness checks
3. Images tagged with version and `latest`
4. Blue-green deployment to production
5. Health checks and monitoring alerts

#### Q: How do I run the same tests that CI runs?

**A: Use the same Docker commands locally.**

```bash
# Run the exact same tests as CI
docker-compose -f docker-compose.test.yml up --build --abort-on-container-exit

# Run specific test categories
docker-compose -f docker-compose.test.yml run --rm backend-test \
  dotnet test --filter Category=Unit

# Run with the same environment variables as CI
docker-compose -f docker-compose.test.yml run --rm \
  -e ASPNETCORE_ENVIRONMENT=Testing \
  backend-test dotnet test
```

#### Q: How do I deploy to a new environment?

**A: Create environment-specific Docker Compose files.**

1. **Create environment file**:
```yaml
# docker-compose.newenv.yml
version: '3.8'
services:
  backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=NewEnv
      - ConnectionStrings__DefaultConnection=Host=newenv-postgres;Port=5432;Database=dashboarddb;Username=postgres;Password=secure-password

  postgres:
    environment:
      POSTGRES_PASSWORD: secure-password
```

2. **Deploy**:
```bash
docker-compose -f docker-compose.yml -f docker-compose.newenv.yml up -d
```

3. **Add to CI/CD pipeline**:
```yaml
# .github/workflows/deploy-newenv.yml
- name: Deploy to New Environment
  run: |
    docker-compose -f docker-compose.yml -f docker-compose.newenv.yml up -d
```

### Docker-Specific Questions

#### Q: Why use Docker for development instead of installing everything locally?

**A: Multiple compelling reasons.**

**Environment Consistency:**
- "It works on my machine" is eliminated
- New developers productive in minutes, not hours
- Exact same environment from development to production

**Dependency Management:**
- No conflicts between different .NET versions
- PostgreSQL version exactly matches production
- All team members use identical tool versions

**Easy Cleanup:**
- Remove entire environment with one command
- No leftover files cluttering your system
- Easy to try different configurations

**Realistic Testing:**
- Test with the same networking as production
- Database connections identical to production
- Container resource limits simulate production constraints

#### Q: Does Docker make development slower?

**A: Initially slower, then much faster.**

**Initial Learning Curve:**
- First-time Docker image downloads (one-time cost)
- Learning Docker commands
- Understanding container concepts

**Long-term Benefits:**
- No time spent on environment setup
- No debugging environment-specific issues
- Faster onboarding for new team members
- No time lost to "works on my machine" problems

**Performance Tips:**
```bash
# Use Docker BuildKit for faster builds
export DOCKER_BUILDKIT=1

# Use volume mounts for live reloading
volumes:
  - ./Backend:/app
  - /app/bin  # Exclude binary folders
  - /app/obj

# Use Docker layer caching
RUN dotnet restore  # This layer is cached when dependencies don't change
COPY . .           # Only this layer rebuilds when code changes
```

#### Q: How do I handle database data persistence?

**A: Different strategies for different needs.**

**Development (Disposable Data):**
```bash
# Data resets with container restart - perfect for development
docker-compose down -v  # Removes data
docker-compose up -d    # Fresh data from seed
```

**Staging/Production (Persistent Data):**
```yaml
# docker-compose.prod.yml
services:
  postgres:
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups

volumes:
  postgres_data:
    driver: local
```

**Backup and Restore:**
```bash
# Backup
docker exec dashboard-postgres pg_dump -U postgres dashboarddb > backup.sql

# Restore
cat backup.sql | docker exec -i dashboard-postgres psql -U postgres -d dashboarddb
```

#### Q: How do I handle secrets and environment variables?

**A: Different approaches for different environments.**

**Development:**
```yaml
# docker-compose.yml (OK for development)
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=dashboarddb;Username=postgres;Password=postgres123
```

**Production:**
```yaml
# docker-compose.prod.yml
environment:
  - ConnectionStrings__DefaultConnection=${DATABASE_CONNECTION_STRING}
env_file:
  - .env.production  # Never commit this file
```

**CI/CD:**
```yaml
# GitHub Actions
env:
  DATABASE_CONNECTION_STRING: ${{ secrets.DATABASE_CONNECTION_STRING }}
```

### Performance and Monitoring Questions

#### Q: How do I monitor the application performance?

**A: Multiple monitoring layers.**

**Basic Health Checks:**
```bash
# Check if services are running
docker-compose ps

# Check resource usage
docker stats

# Application health endpoint
curl http://localhost:8080/health
```

**Application Logs:**
```bash
# Structured logging with Docker
docker-compose logs -f backend | grep ERROR

# Log aggregation for production
# Use tools like ELK stack or Grafana
```

**Database Monitoring:**
```bash
# PostgreSQL performance
docker exec dashboard-postgres psql -U postgres -d dashboarddb \
  -c "SELECT * FROM pg_stat_activity;"

# Database size and connections
docker exec dashboard-postgres psql -U postgres -d dashboarddb \
  -c "SELECT pg_size_pretty(pg_database_size('dashboarddb'));"
```

#### Q: How do I optimize Docker build times?

**A: Use multi-stage builds and layer caching.**

**Optimized Dockerfile:**
```dockerfile
# Backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies first (better caching)
COPY ["*.csproj", "."]
RUN dotnet restore

# Copy source code last (changes more frequently)
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DashboardApi.dll"]
```

**Build Optimization:**
```bash
# Use BuildKit for parallel builds
export DOCKER_BUILDKIT=1

# Build with cache
docker build --cache-from registry/dashboard-backend:latest .

# Multi-platform builds (if needed)
docker buildx build --platform linux/amd64,linux/arm64 .
```

### Troubleshooting Questions

#### Q: Service won't start, how do I debug?

**A: Systematic debugging approach.**

**Step 1: Check service status**
```bash
docker-compose ps
```

**Step 2: Check logs**
```bash
docker-compose logs backend
docker-compose logs postgres
```

**Step 3: Check networking**
```bash
# Test inter-container communication
docker exec dashboard-frontend ping backend
docker exec dashboard-backend ping postgres
```

**Step 4: Check environment variables**
```bash
docker exec dashboard-backend env | grep CONNECTION
```

**Step 5: Check database connectivity**
```bash
# Test database connection
docker exec dashboard-backend dotnet ef database update --dry-run
```

#### Q: Database connection refused errors?

**A: Common causes and solutions.**

**Check 1: Service startup order**
```yaml
# Ensure proper dependency order
services:
  backend:
    depends_on:
      postgres:
        condition: service_healthy

  postgres:
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5
```

**Check 2: Connection string**
```bash
# Verify connection string in backend
docker exec dashboard-backend env | grep DefaultConnection

# Should use Docker service name 'postgres', not 'localhost'
# ✅ Host=postgres;Port=5432
# ❌ Host=localhost;Port=5432
```

**Check 3: Database readiness**
```bash
# Test PostgreSQL directly
docker exec dashboard-postgres pg_isready -U postgres

# Check if database exists
docker exec dashboard-postgres psql -U postgres -l
```

#### Q: Changes to code not reflecting in the application?

**A: Most likely forgot to rebuild containers.**

**Check 1: Did you rebuild after changing code?**
```bash
# REQUIRED after every code change
docker-compose up --build -d

# Or rebuild specific service
docker-compose up --build backend -d
```

**Check 2: Check if rebuild completed successfully**
```bash
# Watch build progress
docker-compose up --build

# Check for build errors
docker-compose logs backend
```

**Check 3: Browser caching (for frontend changes)**
```bash
# Clear browser cache or hard refresh (Ctrl+F5)
```

**Check 4: Force clean rebuild**
```bash
# Nuclear option - rebuild everything from scratch
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

**Remember: This setup requires rebuilds because:**
- Code is copied into containers during build (not volume mounted)
- Changes only apply after rebuild completes
- This is the trade-off for production parity

This comprehensive guide should help developers understand the full development lifecycle, from local development to production deployment, all using Docker and modern CI/CD practices.