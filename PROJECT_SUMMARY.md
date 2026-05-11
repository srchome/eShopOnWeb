# eShopOnWeb Project Summary

## Overview

**eShopOnWeb** is a Microsoft reference application demonstrating enterprise-level ASP.NET Core web application development. It's a simplified e-commerce store designed to showcase modern architectural patterns, best practices, and deployment strategies for building scalable web applications.

The project is maintained by [NimblePros](https://nimblepros.com/) and serves as a companion to the free eBook: *"Architecting Modern Web Applications with ASP.NET Core and Azure"* (updated to ASP.NET Core 10.0).

### Key Characteristics
- **Monolithic architecture** - Single-process application (not microservices)
- **Clean Architecture** - Clear separation of concerns with domain-driven design
- **Enterprise patterns** - Repository, Specification, Mediator, and Result patterns
- **Production-ready** - Includes comprehensive testing, logging, and monitoring
- **Cloud-native** - Built for Azure deployment with .NET Aspire orchestration

---

## Technical Stack

### Core Framework
| Component | Version | Purpose |
|-----------|---------|---------|
| **.NET Runtime** | 10.0 | Latest .NET runtime |
| **ASP.NET Core** | 10.0 | Web framework |
| **C#** | Latest | Primary language with nullable reference types |

### Frontend Technologies
| Technology | Purpose |
|-----------|---------|
| **Razor Pages** | Main UI for catalog and shopping |
| **ASP.NET Core MVC** | Controllers and views for traditional web pages |
| **Blazor WebAssembly** | Admin dashboard (client-side) |
| **HTML/CSS/JavaScript** | Client-side markup and styling |

### Backend & Data Access
| Technology | Version | Purpose |
|-----------|---------|---------|
| **Entity Framework Core** | 10.0 | ORM for database access |
| **SQL Server** | 2022 | Primary relational database |
| **In-Memory Database** | 10.0 | Testing and development alternative |

### Key Libraries & Frameworks

**Architecture & Patterns:**
- **MediatR** (12.4.1) - Mediator pattern for commands/queries
- **Ardalis.Specification** (9.2.0) - Specification pattern for queries
- **Ardalis.ApiEndpoints** (4.1.0) - Organized API endpoint structure
- **Ardalis.GuardClauses** (5.0.0) - Input validation
- **Ardalis.Result** (10.1.0) - Functional error handling
- **AutoMapper** (12.0.1) - Object-to-object mapping

**API & Validation:**
- **FastEndpoints** (6.1.0) - Modern API endpoint framework
- **FluentValidation** (11.11.0) - Fluent validation rules
- **FastEndpoints.Swagger** (6.1.0) - OpenAPI/Swagger documentation

**Authentication & Security:**
- **ASP.NET Core Identity** - User management and authentication
- **JWT Bearer** (10.0.0) - Token-based authentication
- **GitHub OAuth** - Social login integration
- **Azure.Identity** (1.14.2) - Azure authentication
- **Azure.Extensions.AspNetCore.Configuration.Secrets** (1.4.0) - Key Vault integration

**Observability & Monitoring:**
- **OpenTelemetry** (1.14.0) - Distributed tracing and metrics
- **Seq** (13.0.1) - Structured logging
- **NimblePros.Metronome** (0.4.1) - Distributed tracing

**Cloud & Orchestration:**
- **.NET Aspire** (13.0.1) - Cloud-native orchestration
- **Microsoft.Extensions.ServiceDiscovery** (10.0.0) - Service discovery
- **Microsoft.Extensions.Http.Resilience** (10.0.0) - Resilient HTTP calls

**Testing:**
- **xUnit** (3.2.1) - Unit testing framework
- **MSTest** (4.0.2) - Alternative testing framework
- **NSubstitute** (5.3.0) - Mocking library
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing utilities

**Deployment:**
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets** (1.21.0) - Container tooling

---

## Project Structure

### Solution Organization

```
eShopOnWeb/
├── src/                          # Source code
│   ├── Web/                      # Main ASP.NET Core MVC/Razor Pages app
│   ├── ApplicationCore/          # Domain layer (business logic)
│   ├── Infrastructure/           # Data access layer (EF Core)
│   ├── PublicApi/                # RESTful API service
│   ├── BlazorAdmin/              # Blazor WebAssembly admin interface
│   ├── BlazorShared/             # Shared Blazor components
│   ├── eShopWeb.AppHost/         # .NET Aspire orchestration host
│   └── eShopWeb.AspireServiceDefaults/  # Aspire configuration
├── tests/                        # Test projects
│   ├── UnitTests/                # Domain and service logic tests
│   ├── IntegrationTests/         # Database and repository tests
│   ├── FunctionalTests/          # End-to-end application tests
│   └── PublicApiIntegrationTests/# API endpoint tests
├── docs/                         # Documentation
├── infra/                        # Infrastructure as Code (Bicep)
└── docker-compose.yml            # Docker Compose configuration
```

### Core Projects

#### 1. **Web** (Main Application)
- ASP.NET Core MVC controllers and Razor views
- Razor Pages for catalog browsing and shopping cart
- Admin area with Blazor integration
- Health checks and middleware
- Configuration and startup

#### 2. **ApplicationCore** (Domain Layer)
- **Entities**: CatalogItem, Order, Basket, Buyer, CatalogBrand
- **Aggregates**: Order and Basket aggregates (DDD)
- **Specifications**: Query patterns for data access
- **Services**: BasketService, OrderService
- **Interfaces**: Repository and service contracts
- **No infrastructure dependencies** - Pure business logic

#### 3. **Infrastructure** (Data Access Layer)
- Entity Framework Core DbContexts
  - `CatalogContext` - Product catalog and shopping cart
  - `AppIdentityDbContext` - User credentials and identity
- Repository implementations
- Database migrations
- Identity store implementations

#### 4. **PublicApi** (REST API)
- FastEndpoints-based API structure
- JWT authentication
- Swagger/OpenAPI documentation
- Separate from Web for independent scaling
- Integration with ApplicationCore services

#### 5. **BlazorAdmin** (Admin Dashboard)
- Blazor WebAssembly client-side application
- Admin interface for managing products and orders
- Communicates with PublicApi

#### 6. **eShopWeb.AppHost** (.NET Aspire)
- Orchestrates all services
- Service discovery and configuration
- Local development orchestration

### Test Projects

- **UnitTests**: Tests for domain logic and services
- **IntegrationTests**: Database and repository tests
- **FunctionalTests**: End-to-end application tests
- **PublicApiIntegrationTests**: API endpoint tests

---

## Architecture Patterns

### Clean Architecture
The application follows Clean Architecture principles with clear dependency flow:
- **ApplicationCore** (center) - Domain and business logic
- **Infrastructure** - Data access and external services
- **Web** - User interface and API
- Dependencies point inward; outer layers depend on inner layers

### Design Patterns Implemented

| Pattern | Purpose | Implementation |
|---------|---------|-----------------|
| **Repository** | Abstract data access | Generic repository with EF Core |
| **Specification** | Encapsulate query logic | Ardalis.Specification package |
| **Mediator** | Decouple commands/queries | MediatR for command handling |
| **MVC** | Web UI presentation | ASP.NET Core MVC controllers and views |
| **Razor Pages** | Simplified page-based UI | ASP.NET Core Razor Pages |
| **API Endpoints** | Organized API structure | FastEndpoints framework |
| **Guard Clauses** | Input validation | Ardalis.GuardClauses |
| **Result Pattern** | Functional error handling | Ardalis.Result for service returns |
| **Aggregate Pattern** | Domain-driven design | Order and Basket aggregates |

---

## How to Run the Project

### Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server 2022** - (or use in-memory database for development)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Docker & Docker Compose** (optional, for containerized deployment)
- **Git** - For cloning the repository

### Option 1: Local Development (Recommended for Development)

#### Step 1: Clone and Restore
```bash
git clone https://github.com/nimblepros/eShopOnWeb.git
cd eShopOnWeb
dotnet restore
```

#### Step 2: Database Setup (SQL Server)

If using SQL Server:
```bash
# Install or update Entity Framework Core tools
dotnet tool update --global dotnet-ef

# Navigate to Web project
cd src/Web

# Apply migrations to create databases
dotnet ef database update -c catalogcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj
dotnet ef database update -c appidentitydbcontext -p ../Infrastructure/Infrastructure.csproj -s Web.csproj
```

If using in-memory database (no setup needed):
- Add to `appsettings.json`: `"UseOnlyInMemoryDatabase": true`

#### Step 3: Run the Application

**Option A: Run Web only**
```bash
cd src/Web
dotnet run --launch-profile https
```
- Access at: `https://localhost:5001/`
- Admin area: `https://localhost:5001/admin`

**Option B: Run Web + PublicApi**
```bash
# Terminal 1: Start PublicApi
cd src/PublicApi
dotnet run

# Terminal 2: Start Web
cd src/Web
dotnet run --launch-profile https
```

#### Step 4: Access the Application
- **Web Application**: https://localhost:5001/
- **Admin Area**: https://localhost:5001/admin
- **API Swagger**: http://localhost:5200/swagger (if PublicApi running)

---

### Option 2: Docker Compose (Containerized)

#### Step 1: Build and Run
```bash
docker-compose build
docker-compose up
```

#### Step 2: Access Services
- **Web Application**: http://localhost:5106/
- **PublicApi**: http://localhost:5200/
- **SQL Server**: localhost:1433
  - Username: `sa`
  - Password: `@someThingComplicated1234`

#### Step 3: Stop Services
```bash
docker-compose down
```

---

### Option 3: .NET Aspire (Cloud-Native Orchestration)

#### Step 1: Run Aspire Host
```bash
dotnet run --project src/eShopWeb.AppHost
```

#### Step 2: Access Dashboard
- **Aspire Dashboard**: http://localhost:18888 (typically)
- All services are orchestrated and discoverable

**Benefits:**
- Automatic service discovery
- Integrated logging and tracing
- Easy local development of distributed systems

---

### Option 4: Dev Containers (VS Code)

#### Step 1: Prerequisites
- Install [VS Code Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
- Install Docker Desktop

#### Step 2: Open in Container
- Open the project folder in VS Code
- Click "Reopen in Container" when prompted
- Or use Command Palette: `Dev Containers: Reopen in Container`

#### Step 3: Run Application
```bash
cd src/Web
dotnet run --launch-profile https
```

**Benefits:**
- Consistent development environment
- All dependencies pre-configured
- Works on Windows, Mac, and Linux

---

### Option 5: Azure Deployment (Production)

#### Step 1: Install Azure Developer CLI
```bash
# Windows
powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"

# Linux/MacOS
curl -fsSL https://aka.ms/install-azd.sh | bash
```

#### Step 2: Initialize and Deploy
```bash
azd auth login
azd init -t NimblePros/eShopOnWeb
azd up
```

**What it does:**
- Provisions Azure resources (App Service, SQL Database, Key Vault)
- Deploys the application
- Configures secrets in Azure Key Vault
- Sets up monitoring and logging

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/UnitTests/UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests.csproj
dotnet test tests/FunctionalTests/FunctionalTests.csproj
```

### Run with Code Coverage
```bash
dotnet test --settings CodeCoverage.runsettings
```

### Test Projects
- **UnitTests** - Fast, isolated tests for domain logic
- **IntegrationTests** - Database and repository tests
- **FunctionalTests** - End-to-end application tests
- **PublicApiIntegrationTests** - API endpoint tests

---

## Configuration

### Application Settings

Edit `src/Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CatalogConnection": "Server=.;Database=eShopOnWeb;Trusted_Connection=true;",
    "Identity": "Server=.;Database=eShopOnWeb_Identity;Trusted_Connection=true;"
  },
  "UseOnlyInMemoryDatabase": false,
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### User Secrets (Development)

Store sensitive data locally:
```bash
cd src/Web
dotnet user-secrets init
dotnet user-secrets set "GitHub:ClientId" "your-client-id"
dotnet user-secrets set "GitHub:ClientSecret" "your-client-secret"
```

### Azure Key Vault (Production)

Secrets are automatically loaded from Azure Key Vault when deployed to Azure.

---

## Key Features

### E-Commerce Functionality
- **Product Catalog** - Browse and search products
- **Shopping Cart** - Add/remove items, manage quantities
- **Orders** - Place orders, view order history
- **User Accounts** - Registration, login, profile management

### Admin Features
- **Product Management** - Add, edit, delete products
- **Order Management** - View and manage orders
- **User Management** - Manage user roles and permissions
- **Blazor Admin Dashboard** - Modern admin interface

### Technical Features
- **Authentication** - Local accounts and GitHub OAuth
- **Authorization** - Role-based access control
- **API** - RESTful API with Swagger documentation
- **Logging** - Structured logging with Seq
- **Monitoring** - OpenTelemetry tracing and metrics
- **Health Checks** - Application health endpoints

---

## Default Credentials

### Demo User
- **Email**: demouser@microsoft.com
- **Password**: (Set during first run or check documentation)

### SQL Server (Docker)
- **Username**: `sa`
- **Password**: `@someThingComplicated1234`

---

## Key Ports

| Service | Port | Protocol |
|---------|------|----------|
| Web Application | 5001 | HTTPS |
| Web Application | 5000 | HTTP |
| PublicApi | 5200 | HTTP |
| SQL Server | 1433 | TCP |
| Seq (Logging) | 5341 | HTTP |
| Aspire Dashboard | 18888 | HTTP |

---

## Documentation

- **Getting Started**: [Getting Started for Beginners](https://nimblepros.github.io/eShopOnWeb/getting-started-for-beginners.html)
- **Architecture**: [Architecture Documentation](docs/explore/architecture.md)
- **Patterns**: [Design Patterns](docs/explore/patterns.md)
- **FAQ**: [Frequently Asked Questions](https://github.com/nimblepros/eShopOnWeb/wiki/Frequently-Asked-Questions)
- **eBook**: [Architecting Modern Web Applications with ASP.NET Core and Azure](https://aka.ms/webappebook)

---

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Check connection strings in `appsettings.json`
- Verify database migrations have been applied

### Port Already in Use
- Change ports in `launchSettings.json` (Web)
- Use different terminal windows for multiple services

### Docker Issues
- Ensure Docker Desktop is running
- Check Docker logs: `docker-compose logs`
- Rebuild containers: `docker-compose build --no-cache`

### Missing Dependencies
- Restore NuGet packages: `dotnet restore`
- Update .NET SDK: `dotnet sdk check`

---

## Related Resources

- **GitHub Repository**: https://github.com/nimblepros/eShopOnWeb
- **eShop Microservices Sample**: https://github.com/dotnet/eShop
- **Reliable Web App Pattern**: https://learn.microsoft.com/azure/architecture/web-apps/guides/reliable-web-app/overview
- **Clean Architecture**: https://deviq.com/design-patterns/clean-architecture
- **NimblePros Academy**: https://academy.nimblepros.com/

---

## Summary

**eShopOnWeb** is a comprehensive reference application for building enterprise-grade ASP.NET Core web applications. It demonstrates:

✅ Clean Architecture principles  
✅ Domain-driven design patterns  
✅ Comprehensive testing strategies  
✅ Modern deployment practices  
✅ Cloud-native development with .NET Aspire  
✅ Production-ready security and monitoring  

Whether you're learning ASP.NET Core or building enterprise applications, eShopOnWeb provides a solid foundation and best practices to follow.
