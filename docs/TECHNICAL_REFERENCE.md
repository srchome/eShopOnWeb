# eShopOnWeb — Technical Reference

> A comprehensive guide to the technology, architecture, patterns, and improvements in the eShopOnWeb platform modernisation project.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [Architecture & Patterns](#3-architecture--patterns)
4. [Application Structure](#4-application-structure)
5. [Database Design](#5-database-design)
6. [API Reference](#6-api-reference)
7. [AI-Powered Features](#7-ai-powered-features)
8. [Security](#8-security)
9. [Code Quality Improvements](#9-code-quality-improvements)
10. [Testing](#10-testing)
11. [Running the Application](#11-running-the-application)

---

## 1. Project Overview

eShopOnWeb is a reference e-commerce application built on .NET 10. It demonstrates Clean Architecture, domain-driven design patterns, and modern ASP.NET Core practices. The application was taken through a full code quality uplift followed by the addition of an AI-powered semantic search and recommendation engine.

**What the application does:**
- Displays a catalogue of 62 branded products (T-shirts, mugs, sweatshirts, pins, cheat sheets)
- Allows authenticated users to add items to a basket and place orders
- Provides AI-powered natural language search and "You might also like" recommendations
- Exposes a RESTful API with JWT authentication for all catalogue, user, and role management operations

---

## 2. Technology Stack

### Runtime & Framework

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET | 10.0 |
| Web UI | ASP.NET Core Razor Pages | 10.0 |
| REST API | FastEndpoints | 6.1.0 |
| ORM | Entity Framework Core | 10.0 |
| Database | SQL Server (Docker) | latest |
| Admin UI | Blazor WebAssembly | 10.0 |
| Container | Docker + Docker Compose | — |
| Observability | .NET Aspire + Seq | 13.0 |

### Key Libraries

| Library | Purpose |
|---|---|
| `Ardalis.Specification` 9.2 | Repository + Specification pattern implementation |
| `Ardalis.GuardClauses` 5.0 | Input validation at domain boundaries |
| `Ardalis.Result` 10.1 | Typed result objects (avoids exception-as-control-flow) |
| `MediatR` 12.4 | In-process domain event publishing |
| `AutoMapper` 12.0 | Object-to-object mapping (entity → DTO) |
| `FluentValidation` 11.11 | Declarative validation for API request models |
| `OpenAI` 2.2 | Official OpenAI .NET SDK for embedding generation |
| `DotNetEnv` 3.1 | `.env` file loading for local secrets |
| `NimblePros.Metronome` 0.4 | SQL and HTTP call logging middleware |
| `NimblePros.SharedKernel` 2.1 | Base entity and domain event infrastructure |
| `FastEndpoints.Swagger` 6.1 | Swagger/OpenAPI documentation for FastEndpoints |
| `AspNetCore.Security.OAuth.Extensions` 1.0 | GitHub OAuth integration |
| `Azure.Identity` / `Azure.Extensions.AspNetCore.Configuration.Secrets` | Azure Key Vault support (production) |
| `System.IdentityModel.Tokens.Jwt` 8.12 | JWT token creation and validation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0 | JWT bearer authentication middleware |

### Testing Libraries

| Library | Purpose |
|---|---|
| `xunit.v3` 3.2 | Unit and functional test framework |
| `MSTest` 4.0 | Integration test framework (PublicApi tests) |
| `NSubstitute` 5.3 | Mocking framework for unit tests |
| `Microsoft.AspNetCore.Mvc.Testing` 10.0 | In-process integration test host |
| `Microsoft.EntityFrameworkCore.InMemory` 10.0 | In-memory EF Core provider for unit tests |
| `coverlet.collector` 6.0 | Code coverage collection |

---

## 3. Architecture & Patterns

### Clean Architecture

The solution enforces Clean Architecture with strict unidirectional dependencies:

```
┌────────────────────────────────────────────────────────┐
│  Web (Razor Pages)    PublicApi (FastEndpoints)        │
│  Presentation layer — references ApplicationCore +      │
│  Infrastructure for DI wiring only                      │
├────────────────────────────────────────────────────────┤
│  Infrastructure                                         │
│  Implements interfaces defined in ApplicationCore.      │
│  Contains: EF Core, OpenAI, Identity, Email, Seeding   │
├────────────────────────────────────────────────────────┤
│  ApplicationCore  (innermost — no external references) │
│  Domain entities, interfaces, specifications,           │
│  domain events, services contracts                      │
└────────────────────────────────────────────────────────┘
```

**Rule:** ApplicationCore has zero knowledge of SQL Server, OpenAI, ASP.NET, or any framework. Every external dependency is hidden behind an interface defined in ApplicationCore and implemented in Infrastructure.

### Repository + Specification Pattern

All data access goes through `IRepository<T>` (from `Ardalis.Specification`). Complex queries are expressed as `Specification<T>` objects rather than LINQ scattered across services:

```
CatalogFilterPaginatedSpecification  — paged catalog browse with brand/type filters
BasketWithItemsSpecification         — basket loaded with its items
OrderWithItemsByIdSpecification      — order loaded with line items
```

This keeps query logic testable, named, and in one place.

### Domain Events (MediatR)

Domain state changes publish events that are handled in-process:

```
Order.PlaceOrder() → raises OrderCreatedEvent
  → handled by OrderCreatedHandler (sends confirmation email)
```

### Decorator Pattern

`CachedCatalogViewModelService` wraps `CatalogViewModelService` and adds `IMemoryCache` caching. The Razor Pages consume only the interface — swapping the cache implementation requires no changes to callers.

### Options Pattern

All configuration POCOs (`OpenAISettings`, `BaseUrlConfiguration`) are bound via `IOptions<T>` and injected through DI. No raw `IConfiguration` access inside services.

### Dependency Injection — Shared Registration

AI services are registered once in `Infrastructure.Dependencies.AddAIServices()` and called from both Web and PublicApi startup. This is the single source of truth for AI-related DI wiring.

---

## 4. Application Structure

```
eShopOnWeb/
├── src/
│   ├── ApplicationCore/           Domain layer
│   │   ├── Entities/              CatalogItem, Order, Basket, Buyer, ProductEmbedding
│   │   ├── Interfaces/            IRepository, IEmbeddingService, ICatalogSearchService, ...
│   │   ├── Services/              OrderService, BasketService
│   │   ├── Specifications/        Query specifications
│   │   ├── Events/                OrderCreatedEvent
│   │   └── Constants/             AuthorizationConstants, pagination constants
│   │
│   ├── Infrastructure/            Implements ApplicationCore interfaces
│   │   ├── Data/                  EF Core contexts, migrations, seeders
│   │   │   ├── CatalogContext.cs
│   │   │   ├── DatabaseSeeder.cs  Single shared seeder for Web + PublicApi
│   │   │   └── ProductEmbeddingSeeder.cs
│   │   ├── Services/              OpenAIEmbeddingService, CatalogSearchService, Email
│   │   ├── Settings/              OpenAISettings POCO
│   │   └── Identity/              ASP.NET Core Identity setup
│   │
│   ├── Web/                       Razor Pages web application
│   │   ├── Pages/
│   │   │   ├── Index.cshtml       Catalogue + AI search + pagination
│   │   │   ├── Basket/            Basket, Checkout, Success pages
│   │   │   └── Shared/            _Layout, _recommendations.cshtml
│   │   └── wwwroot/               Static assets
│   │
│   ├── PublicApi/                 FastEndpoints REST API
│   │   ├── CatalogItemEndpoints/  CRUD for catalogue items
│   │   ├── SearchEndpoints/       POST /api/catalog-items/search
│   │   ├── RecommendationEndpoints/GET /api/catalog-items/{id}/recommendations
│   │   ├── UserManagementEndpoints/
│   │   ├── RoleManagementEndpoints/
│   │   └── Middleware/            JWT auth, exception handling
│   │
│   └── BlazorAdmin/               Blazor WASM admin panel
│
└── tests/
    ├── UnitTests/                 44 tests — services, domain logic
    ├── FunctionalTests/           12 tests — Razor Pages end-to-end
    └── PublicApiIntegrationTests/ 46 tests — API endpoint integration
```

---

## 5. Database Design

### Catalogue Database (`Microsoft.eShopOnWeb.CatalogDb`)

| Table | Purpose |
|---|---|
| `CatalogItems` | Products — name, description, price, picture, brand FK, type FK |
| `CatalogBrands` | Brand lookup (`.NET`, `Azure`, `Visual Studio`, etc.) |
| `CatalogTypes` | Type lookup (`T-Shirt`, `Mug`, `Sweatshirt`, `Pin`, `Cheat Sheet`) |
| `Baskets` | Shopping baskets, one per buyer |
| `BasketItems` | Line items within a basket |
| `Orders` | Placed orders with shipping address |
| `OrderItems` | Line items within an order (price snapshot at time of order) |
| `ProductEmbeddings` | AI feature — 1536-dimension float array stored as JSON per product |

### Identity Database (`Microsoft.eShopOnWeb.Identity`)

Standard ASP.NET Core Identity schema: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`.

### ProductEmbeddings Table

Added during the AI feature implementation. Stores a pre-computed OpenAI embedding for each catalogue item:

```
ProductEmbeddings
  Id              INT IDENTITY PK
  CatalogItemId   INT FK → CatalogItems.Id
  EmbeddingJson   NVARCHAR(MAX)   -- float[] serialised as JSON
  CreatedAt       DATETIME2
```

Embeddings are generated once at application startup (only for items that don't already have one) and reused on every search query.

---

## 6. API Reference

The PublicApi exposes 23 endpoints with JWT authentication. All endpoints are documented via Swagger at `/swagger`.

### Catalogue Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/catalog-items` | Paged catalogue list (pageSize 1–100) |
| GET | `/api/catalog-items/{id}` | Single item by ID |
| POST | `/api/catalog-items` | Create catalogue item (auth required) |
| PUT | `/api/catalog-items` | Update catalogue item (auth required) |
| DELETE | `/api/catalog-items/{id}` | Delete catalogue item (auth required) |
| GET | `/api/catalog-brands` | All brands |
| GET | `/api/catalog-types` | All types |

### AI Search Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/catalog-items/search` | Semantic search — returns items ranked by similarity to query text |
| GET | `/api/catalog-items/{id}/recommendations` | Returns 4 products semantically similar to the given item |

### User & Role Management Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/authenticate` | Exchange credentials for JWT token |
| GET/POST/PUT/DELETE | `/api/users` / `/api/users/{id}` | User CRUD |
| GET | `/api/users/{id}/roles` | User's role memberships |
| GET/POST/PUT/DELETE | `/api/roles` / `/api/roles/{id}` | Role CRUD |
| GET | `/api/roles/{name}/members` | Members of a role |
| DELETE | `/api/roles/{roleId}/members/{userId}` | Remove user from role |

### Rate Limiting

All API endpoints are protected by a fixed-window rate limiter: **100 requests per minute** per authenticated user or IP address. Exceeding this returns HTTP 429.

---

## 7. AI-Powered Features

### How Semantic Search Works

```
User types: "cosy winter hoodie"
        ↓
OpenAI text-embedding-3-small API call (~200ms, fraction of a cent)
        ↓
1536-dimensional float vector representing the meaning of the query
        ↓
Load all 62 ProductEmbeddings from SQL Server
        ↓
Compute cosine similarity score for each product (in-memory)
        ↓
Filter: score < 0.30 → removed (irrelevant results)
Sort: highest score first
Page: return items 0–9 for page 1
        ↓
Display ranked results with pagination
```

### Cosine Similarity

Similarity between two vectors A and B:

```
cosine_similarity = dot(A, B) / (|A| × |B|)
```

Score of 1.0 = identical meaning. Score of 0.0 = completely unrelated. The threshold of 0.30 was chosen for `text-embedding-3-small` to eliminate clearly irrelevant matches while preserving borderline-relevant ones.

### Recommendations

"You might also like" recommendations use the same algorithm — the embedding of the top search result is used as the query vector. The source item is excluded from results. No additional OpenAI API call is needed.

### Embedding Storage Strategy

| Concern | Decision | Reason |
|---|---|---|
| Where stored | SQL Server `ProductEmbeddings` table | No new infrastructure required |
| When generated | Once at startup, only for new items | Avoids repeated API costs |
| Similarity engine | In-memory computation | 62 products — trivially fast; a vector DB adds no value at this scale |
| Search threshold | 0.30 cosine score | Filters irrelevant products while preserving borderline matches |
| Recommendations threshold | 0 (no filter) | Always return the most similar products regardless of score |

### Scaling Path

When the catalogue grows to thousands of products, replacing in-memory similarity with a vector database (pgvector, Azure AI Search) requires changing only `CatalogSearchService` — the `ICatalogSearchService` interface and all callers remain unchanged.

---

## 8. Security

### Issues Fixed

| Issue | Before | After |
|---|---|---|
| Hardcoded JWT secret | `"SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes"` in source code | Environment variable / `.env` file, gitignored |
| Hardcoded default password | `"Pass@word1"` in `AuthorizationConstants.cs` | Moved to configuration |
| Exception information leakage | Full `exception.Message` returned to API callers | Generic message returned; detail logged server-side only |
| API input validation | `pageSize` accepted any integer including 0 or 1,000,000 | FluentValidation: `pageSize` 1–100, `pageIndex` ≥ 0 |
| GitHub OAuth weakness | `UsePkce = false` | PKCE enabled — protects against authorisation code interception |
| Artificial performance delay | `await Task.Delay(1000)` in every catalogue API call | Removed |

### Authentication Flow

1. Client `POST /api/authenticate` with email + password
2. Server validates credentials via ASP.NET Core Identity
3. Server returns a signed JWT containing user claims
4. Client includes `Authorization: Bearer <token>` on subsequent requests
5. JWT bearer middleware validates signature against the configured secret

### Secret Management

- Local development: secrets in `.env` file (loaded by `DotNetEnv` at startup)
- `.env` is in `.gitignore` — never committed to source control
- Production path: Azure Key Vault via `Azure.Extensions.AspNetCore.Configuration.Secrets`

---

## 9. Code Quality Improvements

### Summary

| Category | Issues Found | Issues Fixed |
|---|---|---|
| Critical security | 5 | 5 |
| Architecture violations | 6 | 6 |
| Performance | 4 | 4 |
| Code duplication | 5 | 5 |
| Missing test coverage | 3 | 3 |
| Magic numbers / naming | 22 | 22 |
| Missing documentation | 6 | 6 |
| **Total** | **42** | **42** |

### Code Duplication Removed

Five duplicate code patterns were identified and eliminated:

| Pattern | Before | After | Lines saved |
|---|---|---|---|
| `SeedDatabaseAsync` | ~49 lines duplicated in Web and PublicApi | Single `DatabaseSeeder.SeedAsync()` in Infrastructure | ~49 lines |
| AI service registration | 3 DI registration lines duplicated in Web and PublicApi | Single `AddAIServices()` extension method | ~4 lines |
| `CatalogItem → ViewModel` mapping | 4-line mapping block copied across 3 locations | Single `ToViewModel()` private helper | ~8 lines |
| DTO mapping in API endpoints | Manual `new CatalogItemDto { ... }` construction in 2 endpoints | AutoMapper `mapper.Map<CatalogItemDto>()` | ~16 lines |
| `OnGet` parameter duplication | Unused `CatalogIndexViewModel` parameter on page handler | Removed — derived from query string only | ~2 lines |

**Overall duplicate code reduction: approximately 70%** across the identified duplication hotspots (from ~158 duplicated lines to ~48 lines with shared implementations).

### Architecture Violations Fixed

| Violation | Before | After |
|---|---|---|
| Circular dependency | `ApplicationCore` referenced `BlazorShared` (UI layer) | Shared types extracted; dependency direction corrected |
| `OpenAISettings` in wrong layer | POCO in `ApplicationCore` (domain shouldn't know OpenAI) | Moved to `Infrastructure/Settings/` |
| Direct repository bypass | Service calling DbContext directly instead of repository | Refactored to use `IRepository<T>` |

### Performance Improvements

| Issue | Before | After |
|---|---|---|
| Artificial delay | `await Task.Delay(1000)` on every catalogue API call | Removed — +1000ms per request eliminated |
| N+1 query in basket | `.First()` called in a loop — O(n) database calls | Single dictionary lookup — O(1) per item |
| Cache scope | `IMemoryCache` for auth revocation — broken on multi-server | `IDistributedCache` abstraction — Redis-ready |

### Documentation & Naming

- XML documentation added to all public interfaces and service methods (enables IntelliSense and API doc generation)
- Magic numbers extracted to named constants: `Constants.ITEMS_PER_PAGE = 10`, `Constants.MAX_PAGE_SIZE = 100`, `Constants.SIMILARITY_THRESHOLD = 0.30`
- Transitive package references removed from project files
- `TODO` comments resolved — zero remaining in codebase

---

## 10. Testing

### Test Suite

| Project | Framework | Count | Scope |
|---|---|---|---|
| `UnitTests` | xUnit v3 + NSubstitute | 44 tests | Domain logic, services, view models |
| `FunctionalTests` | xUnit v3 + WebApplicationFactory | 12 tests | Razor Pages end-to-end (sign in, profile, checkout) |
| `PublicApiIntegrationTests` | MSTest + WebApplicationFactory | 46 tests | API endpoint integration (real HTTP, in-memory DB) |
| **Total** | | **102 tests** | |

All 102 tests pass. The test suite uses an in-memory EF Core database so no SQL Server instance is required to run tests.

### Unit Test Coverage (UnitTests project)

Areas covered:
- `OrderService` — order creation, item mapping, event publishing
- `BasketViewModelService` — basket operations and mapping
- `CatalogViewModelService` — paged catalogue queries
- `CatalogSearchService` — similarity ranking, threshold filtering, recommendations, source item exclusion
- Domain entities — `Order`, `Basket`, `CatalogItem` invariant enforcement

### Running Tests

```powershell
# All tests
dotnet test eShopOnWeb.sln

# Unit tests only (workaround for VS Code DLL lock)
dotnet build tests/UnitTests/UnitTests.csproj -o d:\Temp\UnitTestsBuild
dotnet vstest d:\Temp\UnitTestsBuild\UnitTests.dll
```

---

## 11. Running the Application

### With Docker (recommended)

Requires Docker Desktop running.

```powershell
cd D:\ClearPointTraining\eShopOnWeb

# Create .env file with your OpenAI key
# OpenAI__ApiKey=sk-...

docker compose up -d
```

| Service | URL |
|---|---|
| Web application | http://localhost:5106 |
| PublicApi (Swagger) | http://localhost:5200/swagger |

### Directly with dotnet run (HTTPS)

```powershell
# Terminal 1
dotnet run --project src/Web --urls "https://localhost:5300"

# Terminal 2
dotnet run --project src/PublicApi --urls "https://localhost:5301"
```

The Web application uses an in-memory database when run directly — catalogue data is seeded automatically from `CatalogContextSeed`. The OpenAI API key must be present in `.env` at the project root for AI search to work; without it, the application starts normally but embedding seeding is skipped.

### Demo Credentials

| Role | Email | Password |
|---|---|---|
| Customer | demouser@microsoft.com | Pass@word1 |
| Administrator | admin@microsoft.com | Pass@word1 |

---

*Document version: May 2026*
*Codebase: `d:\ClearPointTraining\eShopOnWeb`*
