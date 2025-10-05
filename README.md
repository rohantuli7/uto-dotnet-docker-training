# Dashboard Application - .NET Core with PostgreSQL

A full-stack dashboard application built with .NET 8.0, featuring a REST API backend, Razor Pages frontend, and PostgreSQL database, all containerized with Docker Compose.

## 🔌 Connection Details

| Service | Internal Port | External Port | Access URL |
|---------|---------------|---------------|------------|
| **Frontend** | 5000 | 5000 | http://localhost:5000 |
| **Backend API** | 8080 | 8080 | http://localhost:8080/api/dashboard |
| **Swagger UI** | 8080 | 8080 | http://localhost:8080/swagger |
| **PostgreSQL** | 5432 | 5432 | localhost:5432 (DB clients) |
| **pgAdmin** | 80 | 5050 | http://localhost:5050 |

### 🔗 Service Communication

- **Browser → Frontend**: Direct HTTP on port 5000
- **Frontend → Backend**: Docker internal DNS (`http://backend:8080`)
- **Backend → PostgreSQL**: Connection string (`Host=postgres;Port=5432;Database=dashboarddb`)
- **pgAdmin → PostgreSQL**: Docker network (`postgres:5432`)
- **PostgreSQL → Volume**: Data persistence (`postgres_data:/var/lib/postgresql/data`)

### 🌐 Network Configuration

```yaml
Network: dashboard-network (bridge driver)
├── dashboard-frontend (container)
├── dashboard-backend (container)
├── dashboard-postgres (container)
└── dashboard-pgadmin (container)
```

## 🏗️ Project Structure

```
uto_docker_dotnet/
├── Backend/                    # .NET Web API
│   ├── Controllers/           # API Controllers
│   ├── Data/                  # Database Context
│   ├── Models/                # Data Models
│   ├── Migrations/            # EF Core Migrations
│   ├── Dockerfile
│   └── DashboardApi.csproj
├── Frontend/                   # Razor Pages Web App
│   ├── Pages/                 # Razor Pages
│   ├── Dockerfile
│   └── DashboardWeb.csproj
├── init-db/                   # PostgreSQL initialization scripts
│   └── init.sql
└── docker-compose.yml         # Docker Compose configuration
```

## 📖 Understanding the Components

### 🎨 Frontend (Razor Pages Web App)
A .NET 8.0 Razor Pages application that provides the user interface for the dashboard. It displays analytics data by making HTTP requests to the backend API. The frontend runs on port 5000 and communicates with the backend using Docker's internal networking.

### ⚙️ Backend (ASP.NET Core Web API)
A .NET 8.0 REST API that handles all business logic and database operations. It exposes CRUD endpoints at `/api/dashboard` and includes Swagger documentation at `/swagger`. The backend uses Entity Framework Core with Npgsql to communicate with PostgreSQL and runs on port 8080.

### 🗄️ Database (PostgreSQL 16)
A PostgreSQL database that stores all dashboard data. The database is automatically initialized on first run using Entity Framework Core's `EnsureCreated()` method, which creates the schema and seeds 5 sample dashboard items. Data persists in a Docker volume named `postgres_data`.

### 🔧 pgAdmin (Database Management UI)
A web-based PostgreSQL administration tool for managing and querying the database. Access it at http://localhost:5050 with credentials `admin@admin.com` / `admin`. Connect to the database using hostname `postgres`, port `5432`, and database credentials.

## 🐳 Running the Application

### Option 1: Run All Services Together (Recommended)

This is the easiest way to run the entire application stack:

```bash
# Build and start all containers (frontend, backend, postgres, pgadmin)
docker-compose up --build -d

# View logs from all services
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove all data (clean slate)
docker-compose down -v
```

**What happens:**
- PostgreSQL starts first and runs health checks
- Backend waits for PostgreSQL to be healthy, then starts and initializes the database
- Frontend starts and connects to the backend
- pgAdmin starts for database management
- All services communicate via the `dashboard-network` Docker network

### Option 2: Run Individual Services

You can start specific services independently:

```bash
# Run only the database
docker-compose up postgres -d

# Run only the backend (requires postgres to be running)
docker-compose up backend -d

# Run only the frontend (requires backend to be running)
docker-compose up frontend -d

# Run only pgAdmin (requires postgres to be running)
docker-compose up pgadmin -d
```

### Option 3: Run Services Using Their Dockerfiles

Build and run each container manually without docker-compose:

**1. Create a Docker network:**
```bash
docker network create dashboard-network
```

**2. Run PostgreSQL:**
```bash
docker run -d \
  --name dashboard-postgres \
  --network dashboard-network \
  -e POSTGRES_PASSWORD=postgres123 \
  -e POSTGRES_DB=dashboarddb \
  -p 5432:5432 \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:16-alpine
```

**3. Build and run the Backend:**
```bash
# Build the backend image
cd Backend
docker build -t dashboard-backend .

# Run the backend container
docker run -d \
  --name dashboard-backend \
  --network dashboard-network \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ConnectionStrings__DefaultConnection="Host=dashboard-postgres;Port=5432;Database=dashboarddb;Username=postgres;Password=postgres123" \
  -p 8080:8080 \
  dashboard-backend
```

**4. Build and run the Frontend:**
```bash
# Build the frontend image
cd Frontend
docker build -t dashboard-frontend .

# Run the frontend container
docker run -d \
  --name dashboard-frontend \
  --network dashboard-network \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_URLS=http://+:5000 \
  -e ApiSettings__BaseUrl=http://dashboard-backend:8080 \
  -p 5000:5000 \
  dashboard-frontend
```

**5. Run pgAdmin (optional):**
```bash
docker run -d \
  --name dashboard-pgadmin \
  --network dashboard-network \
  -e PGADMIN_DEFAULT_EMAIL=admin@admin.com \
  -e PGADMIN_DEFAULT_PASSWORD=admin \
  -p 5050:80 \
  dpage/pgadmin4:latest
```

**Note:** Option 3 is more complex and error-prone. Use Option 1 (docker-compose) for development and testing.

### Verification

After starting the services, verify they're running:

```bash
# Check container status
docker ps

# Test backend API
curl http://localhost:8080/api/dashboard

# Test PostgreSQL connection
docker exec -it dashboard-postgres psql -U postgres -d dashboarddb -c "SELECT * FROM \"DashboardItems\";"
```

Access the application:
- **Frontend:** http://localhost:5000
- **Backend API:** http://localhost:8080/api/dashboard
- **Swagger UI:** http://localhost:8080/swagger
- **pgAdmin:** http://localhost:5050

## 🚀 Quick Start

### Prerequisites

- Docker Desktop installed and running
- Docker Compose (included with Docker Desktop)

### Steps to Run

1. **Navigate to the project directory:**
   ```bash
   cd C:\Users\autos\OneDrive\Desktop\code\uto_docker_dotnet
   ```

2. **Build and start all services:**
   ```bash
   docker-compose up --build
   ```

3. **Access the application:**
   - **Frontend Dashboard:** http://localhost:5000
   - **Backend API:** http://localhost:8080/api/dashboard
   - **Swagger UI:** http://localhost:8080/swagger

4. **Stop the application:**
   ```bash
   docker-compose down
   ```

5. **Stop and remove volumes (clean database):**
   ```bash
   docker-compose down -v
   ```

## 📊 Features

- **Dashboard Frontend:** Beautiful gradient UI displaying dummy analytics data
- **REST API:** Full CRUD operations for dashboard items
- **PostgreSQL Database:** Persistent data storage with Entity Framework Core
- **Auto-Migration:** Database schema automatically created on startup
- **Seed Data:** Pre-populated with 5 sample dashboard items
- **Docker Compose:** Complete multi-container orchestration

## 🛠️ API Endpoints

- `GET /api/dashboard` - Get all dashboard items
- `GET /api/dashboard/{id}` - Get specific dashboard item
- `POST /api/dashboard` - Create new dashboard item
- `PUT /api/dashboard/{id}` - Update dashboard item
- `DELETE /api/dashboard/{id}` - Delete dashboard item

## 🔧 Configuration

### Database Connection

The PostgreSQL connection is configured in `docker-compose.yml`:
- **Host:** postgres
- **Port:** 5432
- **Database:** dashboarddb
- **Username:** postgres
- **Password:** postgres123

### Ports

- **Frontend:** 5000
- **Backend:** 8080
- **PostgreSQL:** 5432

## 📝 Development

### Viewing Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres
```

### Rebuilding After Changes

```bash
docker-compose up --build
```

### Accessing PostgreSQL

```bash
docker exec -it dashboard-postgres psql -U postgres -d dashboarddb
```

## 🗂️ Sample Data

The application comes with 5 pre-populated dashboard items:
1. Sales Revenue - Q1 2024 Revenue
2. Active Users - Monthly Active Users
3. Conversion Rate - Current Conversion Rate
4. Customer Satisfaction - CSAT Score
5. Orders Processed - Total Orders This Month

## 🐳 Docker Services

- **postgres:** PostgreSQL 16 Alpine with health checks
- **backend:** .NET 8.0 Web API with Swagger
- **frontend:** .NET 8.0 Razor Pages application

All services are connected via a custom bridge network for inter-service communication.

## 🔄 Troubleshooting

### Port Already in Use
If ports are already in use, modify them in `docker-compose.yml`:
```yaml
ports:
  - "5001:5000"  # Change 5000 to 5001
```

### Database Connection Issues
Ensure PostgreSQL is healthy:
```bash
docker-compose ps
```

### View Container Status
```bash
docker ps
```

## 📄 License

This project is for demonstration purposes.
