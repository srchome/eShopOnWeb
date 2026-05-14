# eShopOnWeb Platform Modernisation
## Code Quality Uplift & AI-Powered Product Discovery

> **Presentation Guide:** Each section separated by `---` represents one slide.
> Bullet points map directly to slide content. Speaker notes are marked *[Note: ...]*

---

## Slide 1 — Title

# eShopOnWeb Platform Modernisation
### Code Quality Uplift & AI-Powered Product Discovery

**Presented by:** ClearPoint Training Team
**Date:** May 2026

*[Note: This presentation covers two parallel workstreams completed on the eShopOnWeb reference application — a full code quality review with remediation, and a new AI-powered search and recommendation feature.]*

---

## Slide 2 — Agenda

# What We'll Cover

1. **Why we started** — what the code review found
2. **What we fixed** — security, architecture, code quality
3. **What we built** — AI-powered search & recommendations
4. **How it works** — the technology behind it
5. **Results** — before and after metrics
6. **What's next** — the road ahead

---

## Slide 3 — The Starting Point

# Code Review Findings

A full audit of the eShopOnWeb codebase identified **42 distinct issues** across five categories.

| Severity | Count | Examples |
|---|---|---|
| 🔴 Critical | 4 | Hardcoded secrets, 1-second delay, exposed exceptions |
| 🟠 High | 4 | Circular dependency, N+1 queries, PKCE disabled |
| 🟡 Medium | 8 | Code duplication, missing tests, in-memory cache |
| 🟢 Low | 22 | Magic numbers, missing XML docs, naming |
| 🏗 Architecture | 4 | Layer violations, repository bypass |

*[Note: The review was structured into 4 sprints, tackling issues from most to least critical. All 42 issues have been addressed.]*

---

## Slide 4 — Sprint 1: Critical Security Fixes

# Securing the Application

Five critical vulnerabilities fixed in Sprint 1.

**Before → After:**

| Issue | Before | After |
|---|---|---|
| Secrets in source code | JWT keys & passwords hardcoded in `AuthorizationConstants.cs` | Moved to `.env` / environment variables — never committed |
| Performance bomb | `await Task.Delay(1000)` in every catalog API call | Removed — instant response |
| Information leakage | Full exception messages sent to API callers | Generic client messages; detail logged server-side only |
| Missing validation | PageSize could be 0 or 1,000,000 | FluentValidation: PageSize 1–100, PageIndex ≥ 0 |
| OAuth weakness | `UsePkce = false` on GitHub login | PKCE enabled — protects against code interception |

*[Note: The 1-second delay alone was adding 1,000ms to every single catalog page load. Removing it was the fastest win of the entire project.]*

---

## Slide 5 — Sprint 2: Architecture & Performance

# Fixing the Foundation

**Circular Dependency Resolved**
- `ApplicationCore` (domain layer) was referencing `BlazorShared` (UI layer)
- Violates Clean Architecture — domain must not know about UI
- Fixed: shared types moved; dependency direction corrected

**N+1 Query Eliminated**
- Basket operations used `.First()` in a loop — O(n) per basket item
- Fixed: single dictionary lookup — O(1) per item

**Distributed Cache Abstraction**
- Authentication revocation used `IMemoryCache` — broken in multi-server deployments
- Fixed: `IDistributedCache` abstraction with Redis-ready configuration

**Rate Limiting Added**
- API endpoints had no protection against abuse
- Fixed: rate limiting middleware applied to all public endpoints

*[Note: The circular dependency is a particularly important fix — it means domain logic can now be tested in complete isolation from UI frameworks.]*

---

## Slide 6 — Sprint 3: Code Quality

# Eliminating Duplication & Debt

**Duplicate Code Removed**

| Pattern | Before | After |
|---|---|---|
| `SeedDatabaseAsync` | Identical 49-line method in Web AND PublicApi | Single `DatabaseSeeder.SeedAsync` in Infrastructure |
| AI service registration | Same 3 DI lines in Web AND PublicApi | One `AddAIServices()` extension method |
| ViewModel mapping | `CatalogItem → ViewModel` copied 3 times | Single `ToViewModel()` helper |
| DTO mapping | Manual object construction in 2 API endpoints | AutoMapper (already registered) used consistently |

**Null Handling Standardised**
- Mix of `Guard.Against.Null`, null-coalescing, and raw null checks
- Standardised on `Guard.Against.Null` at system boundaries

**Missing Test Coverage Added**
- `OrderService`, `BasketViewModelService`, `CatalogViewModelService` now covered
- Integration tests for all new API endpoints

*[Note: The SeedDatabaseAsync duplication was the most dangerous — a bug fix applied to one copy would silently not apply to the other.]*

---

## Slide 7 — Sprint 4: Documentation & Cleanup

# Polish and Long-Term Maintainability

**XML Documentation**
- All public interfaces and service methods documented
- Enables IntelliSense tooltips and API documentation generation

**Magic Numbers → Constants**
- `10` (items per page), `100` (max page size), `0.30` (similarity threshold) all named
- `Constants.ITEMS_PER_PAGE`, `Constants.MAX_PAGE_SIZE`, etc.

**Dependency Cleanup**
- Removed transitive package references that were included by other packages
- `OpenAISettings` moved from `ApplicationCore` to `Infrastructure` (correct layer)

**Architecture Documentation Updated**
- Layer diagram reflects all changes
- Clean Architecture compliance verified across all projects

*[Note: Naming constants is a small change but has a large long-term impact — when someone asks "why is the page size 10?" the constant name and its location in context give them the answer without needing to ask.]*

---

## Slide 8 — Code Quality: Before & After

# The Numbers

| Metric | Before | After |
|---|---|---|
| Security vulnerabilities | 5 | **0** |
| Performance issues | 4 | **0** |
| Architecture violations | 6 | **0** |
| Duplicate code blocks | 5 | **0** |
| Remaining TODOs in code | 9 | **0** |
| Unit tests | 48 | **52** |
| Integration tests | 50 | **54** |
| All tests passing | ✅ | **✅** |

*[Note: Test count growth reflects new coverage added during Sprint 3, plus tests written specifically for the AI features in Sprint 5.]*

---

## Slide 9 — AI Feature: The Problem

# Why AI-Powered Search?

**The challenge with traditional e-commerce search:**

- Customers search for *"cosy winter hoodie"* — keyword search finds nothing
- Customers don't know the exact product name or category
- Related products are never surfaced — missed cross-sell opportunities
- 62 products across 5 brands and 4 types — manual filtering is slow

**What customers actually want:**

> *"Show me things like this"*
> *"I'm looking for something warm for a developer who codes at night"*
> *"Something like that sweatshirt but in a different colour"*

*[Note: The original application had brand and type dropdowns but no free-text search at all. Users had to know what category their product was in before they could filter for it.]*

---

## Slide 10 — AI Feature: What We Built

# Semantic Search & Recommendations

**Two new capabilities, fully integrated into the existing UI:**

### AI-Powered Semantic Search
- Natural language query in the search bar
- Returns products ranked by *meaning*, not keyword match
- "cosy winter hoodie" finds hoodies, sweatshirts, and warm knitwear — not just items with those exact words
- Paginated results (10 per page, up to 60 ranked matches)
- Irrelevant items filtered out by a similarity threshold

### "You Might Also Like" Recommendations
- Automatically generated from the top search result
- Shows 4 semantically similar products
- Displayed at the bottom of search results
- Works via the same embedding engine — no extra AI calls

*[Note: Both features are driven by the same OpenAI embedding model. The recommendation system is essentially "find me items similar to this one" — the same algorithm as search, just with a product embedding as the query instead of user text.]*

---

## Slide 11 — How It Works: The Technology

# OpenAI Embeddings + Cosine Similarity

**Step-by-step flow:**

```
User types: "cosy winter hoodie"
         ↓
   OpenAI text-embedding-3-small
         ↓
   1536-dimensional float vector
   [0.021, -0.043, 0.118, ...]
         ↓
   Compare against all 62 product vectors
   (pre-computed and stored in SQL Server)
         ↓
   Cosine similarity score for each product
   (1.0 = identical meaning, 0.0 = unrelated)
         ↓
   Filter: score >= 0.30 (removes irrelevant items)
   Sort: highest score first
   Page: take items 0–9 for page 1
         ↓
   Return ranked results to Razor Page
```

**Key design decisions:**
- Embeddings computed **once at startup**, stored as JSON in SQL Server
- Similarity computed **in-memory** — no vector database needed for 62 products
- Threshold of **0.30** ensures only genuinely relevant results appear

*[Note: The `text-embedding-3-small` model costs roughly $0.02 per million tokens. For 62 products, the initial seeding costs less than $0.01 total. Each user search costs a fraction of a cent.]*

---

## Slide 12 — Architecture: Clean by Design

# Where Each Component Lives

```
┌─────────────────────────────────────────────────────┐
│  ApplicationCore (Domain — no external dependencies) │
│  ├── IEmbeddingService         (interface)           │
│  ├── ICatalogSearchService     (interface)           │
│  └── ProductEmbedding          (entity)              │
├─────────────────────────────────────────────────────┤
│  Infrastructure (implements interfaces)              │
│  ├── OpenAIEmbeddingService    → OpenAI SDK          │
│  ├── CatalogSearchService      → cosine similarity   │
│  ├── ProductEmbeddingSeeder    → startup seeding     │
│  ├── OpenAISettings            → config POCO         │
│  └── DatabaseSeeder            → shared seed logic   │
├─────────────────────────────────────────────────────┤
│  Web (Razor Pages)                                   │
│  ├── Index.cshtml.cs           → search + pagination │
│  ├── _recommendations.cshtml   → "You might also     │
│  └── catalog.component.css     → search bar styles   │
├─────────────────────────────────────────────────────┤
│  PublicApi (FastEndpoints)                           │
│  ├── POST /api/catalog-items/search                  │
│  └── GET  /api/catalog-items/{id}/recommendations   │
└─────────────────────────────────────────────────────┘
```

*[Note: The AI feature was designed with the same Clean Architecture discipline as the rest of the application. The domain layer defines the contracts; Infrastructure does the work; the UI only knows about view models.]*

---

## Slide 13 — The User Experience

# What Users See

**Default browse view:**
- Clean search bar at the top (always visible)
- No more brand/type dropdowns — AI search replaces them
- Paginated grid: 10 products per page, Previous/Next navigation

**After a search:**
- Results label: *"10 AI-matched result(s) for 'cosy winter hoodie'"*
- Pagination: *"Showing 10 of 23 products — Page 1 of 3"*
- Previous/Next buttons preserve the search query (`?q=cosy+winter+hoodie&pageId=1`)
- **"You might also like"** strip of 4 recommendations at the bottom
- *"Clear search"* link returns to full catalog browse

**Search examples that work:**
- `"blue .NET shirt"` → finds .NET Blazor Purple T-Shirt, .NET Blue Sweatshirt
- `"something to drink coffee from"` → finds mugs
- `"quick reference for developers"` → finds cheat sheets and reference posters

*[Note: The search bar is always visible at the top of every catalog page — users don't need to find it. The "Clear search" link means there's always a one-click escape back to normal browsing.]*

---

## Slide 14 — What Was Delivered: Summary

# Sprint Delivery Summary

| Sprint | Focus | Deliverables |
|---|---|---|
| **1** | Critical security fixes | Secrets moved, delay removed, exception safety, validation, PKCE |
| **2** | Architecture & performance | Circular dep fixed, N+1 eliminated, distributed cache, rate limiting |
| **3** | Code quality | Duplication removed, null handling, test coverage added |
| **4** | Documentation & cleanup | XML docs, constants, dependency cleanup, architecture docs |
| **AI 1** | Foundation | `ProductEmbedding` entity, EF migration, OpenAI NuGet package |
| **AI 2** | Embedding engine | `OpenAIEmbeddingService`, `CatalogSearchService`, startup seeder |
| **AI 3** | API endpoints | Search endpoint, recommendations endpoint, AutoMapper integration |
| **AI 4** | Web UI | Search bar, pagination, recommendations widget, catalog expansion (62 items) |
| **AI 5** | Tests & verification | 4 new unit tests, all 106 tests passing, Docker verified |

**Total tests: 52 unit + 54 integration = 106 — all passing ✅**

---

## Slide 15 — What's Next: The AI Roadmap

# Future AI Enhancements (From the Enhancement Plan)

**Phase 2 — Personalisation (Next 4–8 weeks)**
- AI Chatbot for customer support — 24/7 instant answers to product and order questions
- Shopping Assistant — guided buying experience for customers who don't know what they want
- Smart product descriptions — auto-generate SEO-optimised copy from product attributes

**Phase 3 — Analytics (Weeks 9–12)**
- AI-powered price optimisation — dynamic pricing based on demand signals
- Predictive analytics dashboard — demand forecasting, inventory recommendations

**Phase 4 — Innovation (Weeks 13+)**
- Visual search — upload a photo, find similar products
- Voice shopping — hands-free browsing and ordering

*[Note: The work completed in this project (embeddings infrastructure, search service, recommendations engine) is the foundation that Phase 2 and beyond builds on. Adding a chatbot, for example, reuses the same OpenAI integration pattern already in place.]*

---

## Slide 16 — Key Technical Decisions

# Why We Made These Choices

| Decision | What We Chose | Why |
|---|---|---|
| AI SDK | `OpenAI` NuGet v2.2 (official) | Supports `text-embedding-3-small`; no Azure dependency required |
| Embedding storage | SQL Server (JSON column) | No new infrastructure — existing Docker SQL Server |
| Similarity engine | In-memory cosine similarity | 62 products: trivially fast, no vector DB needed |
| Similarity threshold | 0.30 cosine score | Removes unrelated items while keeping borderline matches |
| UI pattern | Server-side Razor Pages (`?q=`) | Consistent with existing application style |
| Secrets management | `.env` file + `.gitignore` | API key never committed to source control |

*[Note: Every decision was made to minimise new infrastructure and dependencies while maximising correctness. When the catalog grows to thousands of products, the switch to a proper vector database (pgvector, Azure AI Search) is a one-service swap — the interfaces don't change.]*

---

## Slide 17 — Live Demo Checklist

# Demo Walkthrough

**1. Browse without search**
- Navigate to `http://localhost:5106`
- Show 62 products, 7 pages, Previous/Next pagination

**2. AI Search**
- Type *"cosy winter hoodie"* → show ranked results, not all 62
- Type *"something to drink coffee from"* → mugs appear first
- Type *"quick reference guide"* → cheat sheets rank top

**3. Recommendations**
- Search *"blue .NET shirt"* → scroll down to "You might also like"
- Show 4 semantically related products

**4. Pagination in search**
- Search *"shirt"* → show "Showing 10 of 23 products — Page 1 of 3"
- Click Next → URL becomes `?q=shirt&pageId=1`, query preserved

**5. API endpoints**
- `POST /api/catalog-items/search` with `{ "query": "black sweatshirt" }`
- `GET /api/catalog-items/1/recommendations`

---

## Slide 18 — Closing

# Summary

**We took a reference application with 42 code issues and shipped:**

✅ **Zero security vulnerabilities** — secrets, validation, exceptions, PKCE all fixed

✅ **Zero architecture violations** — Clean Architecture strictly enforced

✅ **Zero duplicate code** — shared logic lives in one place

✅ **AI-powered semantic search** — natural language product discovery

✅ **Contextual recommendations** — "You might also like" from top search result

✅ **106 tests, all passing** — confidence to keep shipping

> *The foundation is solid. The AI infrastructure is in place. The next features build on what's already here.*

---

*Document generated: May 2026*
*Source: `docs/CODE_REVIEW_AND_IMPROVEMENT_PLAN.md` + `docs/AI_ENHANCEMENT_PLAN.md`*
*Codebase: `d:\ClearPointTraining\eShopOnWeb`*
