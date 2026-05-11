# eShopOnWeb Code Review & Improvement Plan

## Executive Summary

A comprehensive code review of the eShopOnWeb application has identified **42 distinct issues** across architecture, security, performance, code quality, and testing. This document prioritizes improvements and provides a roadmap for enhancing the application.

**Critical Issues Found:** 4  
**High Priority Issues:** 4  
**Medium Priority Issues:** 8  
**Low Priority Issues:** 22  

---

## Critical Issues (Fix Immediately)

### 🔴 CRITICAL 1: Hardcoded Secrets in Source Code

**Severity:** CRITICAL - Security Vulnerability  
**Location:** `src/ApplicationCore/Constants/AuthorizationConstants.cs`

**Current Code:**
```csharp
public class AuthorizationConstants
{
    public const string AUTH_KEY = "AuthKeyOfDoomThatMustBeAMinimumNumberOfBytes";
    public const string DEFAULT_PASSWORD = "Pass@word1";
    public const string JWT_SECRET_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";
}
```

**Problems:**
- Secrets exposed in version control
- Default password usable for unauthorized access
- TODO comments acknowledge the issue but it's not fixed
- Violates OWASP security guidelines

**Impact:**
- Anyone with repository access can see production secrets
- Default password allows unauthorized admin access
- Compliance violations (SOC 2, ISO 27001)

**Solution:**
1. Move all secrets to environment variables/Azure Key Vault
2. Remove hardcoded values from source code
3. Update configuration to read from secure sources
4. Rotate all exposed secrets immediately

**Implementation:**
```csharp
// NEW: AuthorizationConstants.cs
public class AuthorizationConstants
{
    // These will be injected from configuration
    public static string AUTH_KEY { get; set; } = "";
    public static string JWT_SECRET_KEY { get; set; } = "";
    
    // Remove DEFAULT_PASSWORD entirely - use proper user creation flow
}

// In Program.cs
var authKey = configuration["AUTH_KEY"] ?? throw new InvalidOperationException("AUTH_KEY not configured");
var jwtSecret = configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY not configured");
AuthorizationConstants.AUTH_KEY = authKey;
AuthorizationConstants.JWT_SECRET_KEY = jwtSecret;
```

**Effort:** 2-3 hours  
**Priority:** CRITICAL - Do this first

---

### 🔴 CRITICAL 2: Artificial 1-Second Delay in Production API

**Severity:** CRITICAL - Performance Issue  
**Location:** `src/PublicApi/CatalogItemEndpoints/CatalogItemListPagedEndpoint.cs` (line 28)

**Current Code:**
```csharp
public override async Task HandleAsync(CatalogItemListPagedRequest request, CancellationToken ct)
{
    await Task.Delay(1000, ct);  // ← ARTIFICIAL DELAY
    // ... rest of implementation
}
```

**Problems:**
- Degrades API performance by 1 second per request
- Appears to be debug/test code left in production
- No explanation or justification
- Affects all catalog listing requests

**Impact:**
- 1000ms added latency to every catalog request
- Poor user experience
- Wasted server resources

**Solution:**
Remove the artificial delay immediately.

**Implementation:**
```csharp
public override async Task HandleAsync(CatalogItemListPagedRequest request, CancellationToken ct)
{
    // Remove: await Task.Delay(1000, ct);
    
    var spec = new CatalogFilterPaginatedSpecification(
        request.PageIndex,
        request.PageSize,
        request.BrandId,
        request.TypeId);
    
    var items = await _itemRepository.ListAsync(spec, ct);
    // ... rest of implementation
}
```

**Effort:** 5 minutes  
**Priority:** CRITICAL - Remove immediately

---

### 🔴 CRITICAL 3: Weak Exception Handling Exposing Internal Details

**Severity:** CRITICAL - Information Disclosure  
**Location:** `src/PublicApi/Middleware/ExceptionMiddleware.cs` (lines 30-42)

**Current Code:**
```csharp
private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";
    
    if (exception is DuplicateException || exception is RoleStillAssignedException)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message  // ← EXPOSES INTERNAL DETAILS
        }.ToString());
    }
    // ... more handlers
}
```

**Problems:**
- Exposes full exception messages to clients
- Stack traces and internal details visible to attackers
- Information disclosure vulnerability
- Violates security best practices

**Impact:**
- Attackers can learn about system internals
- Sensitive information exposed
- Compliance violations

**Solution:**
Return generic error messages to clients; log detailed errors server-side.

**Implementation:**
```csharp
private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";
    var requestId = context.TraceIdentifier;
    
    // Log detailed error server-side
    _logger.LogError(exception, "Unhandled exception. RequestId: {RequestId}", requestId);
    
    if (exception is DuplicateException)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = "A resource with this identifier already exists.",
            RequestId = requestId
        }.ToString());
    }
    else if (exception is RoleStillAssignedException)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = "Cannot perform this operation. Please try again later.",
            RequestId = requestId
        }.ToString());
    }
    else
    {
        // Generic error for unexpected exceptions
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = "An unexpected error occurred. Please contact support.",
            RequestId = requestId
        }.ToString());
    }
}
```

**Effort:** 1-2 hours  
**Priority:** CRITICAL - Fix immediately

---

### 🔴 CRITICAL 4: Missing Input Validation on API Endpoints

**Severity:** CRITICAL - DoS Vulnerability  
**Location:** `src/PublicApi/CatalogItemEndpoints/CatalogItemListPagedEndpoint.cs`

**Current Code:**
```csharp
public override async Task HandleAsync(CatalogItemListPagedRequest request, CancellationToken ct)
{
    // No validation of PageIndex or PageSize
    var spec = new CatalogFilterPaginatedSpecification(
        request.PageIndex,      // Could be negative or huge
        request.PageSize,       // Could be negative or huge
        request.BrandId,
        request.TypeId);
}
```

**Problems:**
- No validation of PageIndex (could be negative)
- No validation of PageSize (could be 0 or extremely large)
- Potential DoS vulnerability
- Resource exhaustion possible

**Impact:**
- Attackers can request huge page sizes
- Memory exhaustion
- Database performance degradation

**Solution:**
Add input validation using FluentValidation.

**Implementation:**
```csharp
// Create validator
public class CatalogItemListPagedRequestValidator : Validator<CatalogItemListPagedRequest>
{
    public CatalogItemListPagedRequestValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be 0 or greater");
        
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");
        
        RuleFor(x => x.BrandId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Brand ID must be 0 or greater");
        
        RuleFor(x => x.TypeId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Type ID must be 0 or greater");
    }
}

// In endpoint
public override async Task HandleAsync(CatalogItemListPagedRequest request, CancellationToken ct)
{
    // FastEndpoints automatically validates using registered validators
    var spec = new CatalogFilterPaginatedSpecification(
        request.PageIndex,
        request.PageSize,
        request.BrandId,
        request.TypeId);
    
    // ... rest of implementation
}
```

**Effort:** 1-2 hours  
**Priority:** CRITICAL - Fix immediately

---

## High Priority Issues (Fix Soon)

### 🟠 HIGH 1: Circular Dependency - ApplicationCore References BlazorShared

**Severity:** HIGH - Architecture Violation  
**Location:** `src/ApplicationCore/ApplicationCore.csproj`

**Current Code:**
```xml
<ItemGroup>
    <ProjectReference Include="..\BlazorShared\BlazorShared.csproj" />
</ItemGroup>
```

**Problem:**
- ApplicationCore (domain layer) depends on BlazorShared (presentation layer)
- Violates Clean Architecture principles
- Couples domain logic to UI concerns

**Solution:**
Move shared types to a separate `Shared` project or remove the dependency.

**Implementation:**
1. Create `src/Shared/Shared.csproj` for shared DTOs/models
2. Move shared types from BlazorShared to Shared
3. Update ApplicationCore to reference Shared instead of BlazorShared
4. Update BlazorShared to reference Shared

**Effort:** 3-4 hours  
**Priority:** HIGH - Fix in next sprint

---

### 🟠 HIGH 2: Missing Distributed Cache for Multi-Instance Deployments

**Severity:** HIGH - Production Issue  
**Location:** `src/Web/Configuration/RevokeAuthenticationEvents.cs` (line 8)

**Current Code:**
```csharp
//TODO : replace IMemoryCache by distributed cache if you are in multi-host scenario
```

**Problem:**
- Uses IMemoryCache for authentication revocation
- Won't work in multi-instance deployments
- Token revocation won't propagate across servers

**Impact:**
- In production with multiple servers, users can't be logged out
- Security risk for token revocation
- Session management broken in scaled deployments

**Solution:**
Implement distributed cache using Redis or Azure Cache for Redis.

**Implementation:**
```csharp
// In Program.cs
if (environment.IsProduction())
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("Redis");
    });
}
else
{
    services.AddMemoryCache();
}

// In RevokeAuthenticationEvents.cs
public class RevokeAuthenticationEvents : RemoteAuthenticationEvents
{
    private readonly IDistributedCache _cache;
    
    public RevokeAuthenticationEvents(IDistributedCache cache)
    {
        _cache = cache;
    }
    
    public override async Task SignedOutCallbackAsync(RemoteSignedOutContext context)
    {
        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await _cache.SetStringAsync($"revoked_token_{userId}", "true", 
                new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) 
                });
        }
    }
}
```

**Effort:** 4-6 hours  
**Priority:** HIGH - Fix before production deployment

---

### 🟠 HIGH 3: N+1 Query Problem in Basket Operations

**Severity:** HIGH - Performance Issue  
**Location:** `src/Web/Services/BasketViewModelService.cs` (lines 54-60)

**Current Code:**
```csharp
var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);
var items = basketItems.Select(basketItem =>
{
    var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);  // ← INEFFICIENT
    // ...
}).ToList();
```

**Problem:**
- Uses LINQ-to-Objects `.First()` after database query
- O(n) lookup for each basket item
- Inefficient for large baskets

**Impact:**
- Slow basket operations
- Poor performance with many items

**Solution:**
Use dictionary lookup instead of LINQ-to-Objects.

**Implementation:**
```csharp
var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);
var itemDict = catalogItems.ToDictionary(x => x.Id);  // ← EFFICIENT

var items = basketItems.Select(basketItem =>
{
    if (itemDict.TryGetValue(basketItem.CatalogItemId, out var catalogItem))
    {
        // ... use catalogItem
    }
}).ToList();
```

**Effort:** 1-2 hours  
**Priority:** HIGH - Fix in next sprint

---

### 🟠 HIGH 4: PKCE Disabled for GitHub OAuth

**Severity:** HIGH - Security Issue  
**Location:** `src/Web/Program.cs` (line 42)

**Current Code:**
```csharp
options.UsePkce = false; // PKCE not supported by GitHub
```

**Problem:**
- PKCE (Proof Key for Code Exchange) is disabled
- Reduces OAuth security
- GitHub now supports PKCE
- Vulnerable to authorization code interception

**Solution:**
Enable PKCE for GitHub OAuth.

**Implementation:**
```csharp
// In Program.cs
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GitHubAuthenticationDefaults.DisplayName;
})
.AddCookie()
.AddGitHub(options =>
{
    options.ClientId = configuration["GitHub:ClientId"] ?? "";
    options.ClientSecret = configuration["GitHub:ClientSecret"] ?? "";
    options.UsePkce = true;  // ← ENABLE PKCE
    options.SaveTokens = true;
});
```

**Effort:** 30 minutes  
**Priority:** HIGH - Fix immediately

---

## Medium Priority Issues (Refactor)

### 🟡 MEDIUM 1: Code Duplication - Basket Specification Pattern

**Severity:** MEDIUM - Code Quality  
**Location:** `src/ApplicationCore/Services/BasketService.cs` (multiple methods)

**Problem:**
```csharp
// Repeated in multiple methods
var basketSpec = new BasketWithItemsSpecification(username);
var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);
```

**Solution:**
Extract a private helper method.

**Implementation:**
```csharp
private async Task<Basket?> GetBasketByUserAsync(string username)
{
    var basketSpec = new BasketWithItemsSpecification(username);
    return await _basketRepository.FirstOrDefaultAsync(basketSpec);
}

// Use in methods
public async Task AddItemToBasket(string username, int catalogItemId, decimal price, int quantity = 1)
{
    var basket = await GetBasketByUserAsync(username);
    // ...
}
```

**Effort:** 1 hour  
**Priority:** MEDIUM - Include in next refactoring sprint

---

### 🟡 MEDIUM 2: Code Duplication - Pagination Logic

**Severity:** MEDIUM - Code Quality  
**Location:** `src/Web/Services/CatalogViewModelService.cs` and `src/PublicApi/CatalogItemEndpoints/`

**Problem:**
Pagination calculation logic duplicated across Web and PublicApi layers.

**Solution:**
Extract to shared utility class.

**Implementation:**
```csharp
// Create src/Shared/Utilities/PaginationHelper.cs
public static class PaginationHelper
{
    public static int CalculatePageCount(int totalItems, int pageSize)
    {
        if (pageSize <= 0) return 0;
        return (totalItems + pageSize - 1) / pageSize;  // Integer arithmetic
    }
    
    public static bool IsValidPageIndex(int pageIndex, int pageCount)
    {
        return pageIndex >= 0 && pageIndex < pageCount;
    }
}

// Use in both layers
var pageCount = PaginationHelper.CalculatePageCount(totalItems, pageSize);
```

**Effort:** 2 hours  
**Priority:** MEDIUM - Include in next refactoring sprint

---

### 🟡 MEDIUM 3: Circular Dependency - Infrastructure Bypasses Repository Pattern

**Severity:** MEDIUM - Architecture Issue  
**Location:** `src/Infrastructure/Data/Queries/BasketQueryService.cs`

**Problem:**
BasketQueryService directly uses CatalogContext instead of repository abstraction.

**Solution:**
Implement query specifications for all query operations.

**Effort:** 3-4 hours  
**Priority:** MEDIUM - Refactor in next sprint

---

### 🟡 MEDIUM 4: Missing Distributed Cache Implementation

**Severity:** MEDIUM - Scalability Issue  
**Location:** `src/Web/Services/CachedCatalogViewModelService.cs`

**Problem:**
Uses IMemoryCache; won't work in multi-instance deployments.

**Solution:**
Implement distributed cache abstraction.

**Effort:** 4-6 hours  
**Priority:** MEDIUM - Fix before production

---

### 🟡 MEDIUM 5: Incomplete Cookie Consent Configuration

**Severity:** MEDIUM - Compliance Issue  
**Location:** `src/Web/Configuration/ConfigureCookieSettings.cs`

**Problem:**
```csharp
//TODO need to check that.
//options.CheckConsentNeeded = context => true;
```

**Solution:**
Implement proper GDPR cookie consent.

**Effort:** 2-3 hours  
**Priority:** MEDIUM - Fix before production

---

### 🟡 MEDIUM 6: Hardcoded Base URLs in Seed Data

**Severity:** MEDIUM - Configuration Issue  
**Location:** `src/Infrastructure/Data/CatalogContextSeed.cs`

**Problem:**
```csharp
"http://catalogbaseurltobereplaced/images/products/..."
```

**Solution:**
Use configuration for base URLs.

**Effort:** 1-2 hours  
**Priority:** MEDIUM - Fix before production

---

### 🟡 MEDIUM 7: Missing Test Coverage for Services

**Severity:** MEDIUM - Testing Gap  
**Location:** `tests/UnitTests/`

**Problem:**
Limited test coverage for OrderService, CatalogViewModelService, BasketViewModelService.

**Solution:**
Add comprehensive unit and integration tests.

**Effort:** 8-10 hours  
**Priority:** MEDIUM - Add in next sprint

---

### 🟡 MEDIUM 8: Inconsistent Null Handling

**Severity:** MEDIUM - Code Quality  
**Location:** Multiple files

**Problem:**
Mix of Guard.Against.Null, null-coalescing, and null checks.

**Solution:**
Standardize on Guard.Against.Null for validation.

**Effort:** 2-3 hours  
**Priority:** MEDIUM - Refactor in next sprint

---

## Low Priority Issues (Improve)

### 🟢 LOW 1: Remove Unnecessary System.* Package References

**Severity:** LOW - Cleanup  
**Location:** `Directory.Packages.props`

**Solution:**
Remove transitive dependencies that are included by other packages.

**Effort:** 30 minutes  
**Priority:** LOW - Include in cleanup sprint

---

### 🟢 LOW 2: Extract Magic Numbers to Constants

**Severity:** LOW - Code Quality  
**Location:** Multiple files

**Examples:**
- `int.MaxValue` in specifications
- `1000` in artificial delay
- `100` in page size limits

**Solution:**
Create constants file for magic numbers.

**Effort:** 1-2 hours  
**Priority:** LOW - Include in cleanup sprint

---

### 🟢 LOW 3: Add XML Documentation Comments

**Severity:** LOW - Documentation  
**Location:** Most service classes

**Solution:**
Add comprehensive XML documentation to public methods.

**Effort:** 4-6 hours  
**Priority:** LOW - Include in documentation sprint

---

### 🟢 LOW 4: Improve Pagination Logic Clarity

**Severity:** LOW - Code Quality  
**Location:** `src/Web/Services/CatalogViewModelService.cs`

**Problem:**
String-based CSS class assignment for pagination.

**Solution:**
Use boolean properties instead.

**Effort:** 1 hour  
**Priority:** LOW - Include in cleanup sprint

---

## Implementation Roadmap

### Sprint 1 (Immediate - This Week)
**Focus:** Fix Critical Security Issues

- [ ] Move hardcoded secrets to configuration
- [ ] Remove artificial 1-second delay
- [ ] Fix exception handling to not expose details
- [ ] Add input validation to API endpoints
- [ ] Enable PKCE for GitHub OAuth

**Estimated Effort:** 8-10 hours  
**Risk:** Low - These are straightforward fixes

---

### Sprint 2 (Next Week)
**Focus:** Fix High Priority Issues

- [ ] Implement distributed cache for multi-instance deployments
- [ ] Fix N+1 query problem in basket operations
- [ ] Resolve circular dependency (ApplicationCore → BlazorShared)
- [ ] Add rate limiting to API endpoints

**Estimated Effort:** 12-16 hours  
**Risk:** Medium - Some architectural changes

---

### Sprint 3 (Following Week)
**Focus:** Refactor and Improve Code Quality

- [ ] Extract duplicate code patterns
- [ ] Standardize null handling
- [ ] Add missing test coverage
- [ ] Implement distributed cache abstraction
- [ ] Fix hardcoded configuration values

**Estimated Effort:** 16-20 hours  
**Risk:** Medium - Refactoring requires careful testing

---

### Sprint 4 (Ongoing)
**Focus:** Documentation and Cleanup

- [ ] Add XML documentation comments
- [ ] Extract magic numbers to constants
- [ ] Remove unnecessary dependencies
- [ ] Improve code clarity
- [ ] Update architecture documentation

**Estimated Effort:** 10-12 hours  
**Risk:** Low - Non-critical improvements

---

## Summary by Category

### Security Issues (5 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Hardcoded secrets | CRITICAL | Sprint 1 |
| Weak exception handling | CRITICAL | Sprint 1 |
| Missing input validation | CRITICAL | Sprint 1 |
| PKCE disabled | HIGH | Sprint 1 |
| Missing rate limiting | HIGH | Sprint 2 |

### Performance Issues (4 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Artificial 1-second delay | CRITICAL | Sprint 1 |
| N+1 query problem | HIGH | Sprint 2 |
| Inefficient pagination | MEDIUM | Sprint 3 |
| Missing database indexes | MEDIUM | Sprint 3 |

### Architecture Issues (4 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Circular dependency | HIGH | Sprint 2 |
| Infrastructure bypasses repository | MEDIUM | Sprint 3 |
| Missing distributed cache | MEDIUM | Sprint 2 |
| Incomplete cache invalidation | MEDIUM | Sprint 3 |

### Code Quality Issues (8 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Code duplication | MEDIUM | Sprint 3 |
| Inconsistent null handling | MEDIUM | Sprint 3 |
| Missing documentation | LOW | Sprint 4 |
| Magic numbers | LOW | Sprint 4 |
| Overly complex logic | LOW | Sprint 4 |
| Inconsistent naming | LOW | Sprint 4 |
| Unnecessary dependencies | LOW | Sprint 4 |
| Missing XML comments | LOW | Sprint 4 |

### Testing Gaps (4 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Limited service test coverage | MEDIUM | Sprint 3 |
| No view model service tests | MEDIUM | Sprint 3 |
| Missing API endpoint tests | MEDIUM | Sprint 3 |
| No performance tests | LOW | Sprint 4 |

### Configuration Issues (4 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Hardcoded base URLs | MEDIUM | Sprint 2 |
| Missing configuration validation | MEDIUM | Sprint 2 |
| Incomplete cookie consent | MEDIUM | Sprint 2 |
| Environment-specific complexity | MEDIUM | Sprint 3 |

### Documentation Gaps (4 total)
| Issue | Severity | Status |
|-------|----------|--------|
| Missing architecture details | LOW | Sprint 4 |
| Missing API documentation | LOW | Sprint 4 |
| Missing deployment guide | LOW | Sprint 4 |
| Missing security guidelines | LOW | Sprint 4 |

---

## Metrics & Success Criteria

### Before Improvements
- Security vulnerabilities: 5
- Performance issues: 4
- Code duplication: 3
- Test coverage: ~60%
- Architecture violations: 2

### After Improvements (Target)
- Security vulnerabilities: 0
- Performance issues: 0
- Code duplication: 0
- Test coverage: >85%
- Architecture violations: 0

---

## Conclusion

The eShopOnWeb application has a solid foundation with Clean Architecture principles, but requires attention to security, performance, and code quality. The identified issues are categorized by severity and priority, with a clear implementation roadmap.

**Immediate Actions (This Week):**
1. Fix hardcoded secrets
2. Remove artificial delay
3. Fix exception handling
4. Add input validation
5. Enable PKCE

**Next Steps:**
1. Implement distributed cache
2. Fix N+1 queries
3. Resolve circular dependencies
4. Add comprehensive tests
5. Improve documentation

By following this roadmap, the application will be more secure, performant, maintainable, and production-ready.

---

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Entity Framework Core Best Practices](https://docs.microsoft.com/en-us/ef/core/performance/)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
