# Secrets Management Guide - Non-Azure Options

## Overview

You can store secrets in several ways without Azure. This guide covers the best options for development and production.

---

## Option 1: User Secrets (Development Only) ⭐ RECOMMENDED FOR DEV

### What It Is
Built-in ASP.NET Core feature that stores secrets locally on your machine, outside the project folder.

### Pros ✅
- Built into .NET - no additional setup
- Secrets never committed to Git
- Perfect for local development
- Works on Windows, Mac, Linux
- Automatically loaded by ASP.NET Core

### Cons ❌
- Development only (not for production)
- Secrets stored on local machine
- Not shared across team members

### Setup Steps

#### Step 1: Initialize User Secrets
```bash
cd src/Web
dotnet user-secrets init
```

This creates a `UserSecretsId` in your `.csproj` file:
```xml
<PropertyGroup>
    <UserSecretsId>eShopOnWeb-dev-secrets</UserSecretsId>
</PropertyGroup>
```

#### Step 2: Store Your Secrets
```bash
# Store OpenAI key
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-actual-key-here"
dotnet user-secrets set "OpenAI:Model" "text-embedding-3-small"
dotnet user-secrets set "OpenAI:Endpoint" "https://api.openai.com/v1"

# Store JWT secrets
dotnet user-secrets set "AuthorizationConstants:JWT_SECRET_KEY" "your-secret-key-here"
dotnet user-secrets set "AuthorizationConstants:AUTH_KEY" "your-auth-key-here"

# Store database connection strings
dotnet user-secrets set "ConnectionStrings:CatalogConnection" "Server=.;Database=eShopOnWeb;Trusted_Connection=true;"
dotnet user-secrets set "ConnectionStrings:Identity" "Server=.;Database=eShopOnWeb_Identity;Trusted_Connection=true;"
```

#### Step 3: Access in Code
```csharp
// In Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// User secrets are automatically loaded in Development
// No additional code needed!

var openAiKey = builder.Configuration["OpenAI:ApiKey"];
var jwtSecret = builder.Configuration["AuthorizationConstants:JWT_SECRET_KEY"];
```

#### Step 4: Verify Secrets Are Stored
```bash
# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "OpenAI:ApiKey"

# Clear all secrets
dotnet user-secrets clear
```

### Where Secrets Are Stored
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\eShopOnWeb-dev-secrets\secrets.json`
- **Mac/Linux**: `~/.microsoft/usersecrets/eShopOnWeb-dev-secrets/secrets.json`

### Example secrets.json
```json
{
  "OpenAI:ApiKey": "sk-proj-your-key",
  "OpenAI:Model": "text-embedding-3-small",
  "OpenAI:Endpoint": "https://api.openai.com/v1",
  "AuthorizationConstants:JWT_SECRET_KEY": "your-secret-key",
  "AuthorizationConstants:AUTH_KEY": "your-auth-key",
  "ConnectionStrings:CatalogConnection": "Server=.;Database=eShopOnWeb;Trusted_Connection=true;",
  "ConnectionStrings:Identity": "Server=.;Database=eShopOnWeb_Identity;Trusted_Connection=true;"
}
```

---

## Option 2: Environment Variables (Development & Production)

### What It Is
Store secrets as environment variables on your system or in Docker.

### Pros ✅
- Works in development and production
- No additional tools needed
- Works with Docker
- Standard practice
- Easy to set per environment

### Cons ❌
- Visible in process list (security concern)
- Need to set on each machine
- Not ideal for many secrets

### Setup Steps

#### Step 1: Set Environment Variables (Windows)
```powershell
# PowerShell
$env:OpenAI__ApiKey = "sk-proj-your-key"
$env:OpenAI__Model = "text-embedding-3-small"
$env:OpenAI__Endpoint = "https://api.openai.com/v1"
$env:AuthorizationConstants__JWT_SECRET_KEY = "your-secret-key"
$env:AuthorizationConstants__AUTH_KEY = "your-auth-key"

# Or set permanently (Windows)
[Environment]::SetEnvironmentVariable("OpenAI__ApiKey", "sk-proj-your-key", "User")
```

#### Step 2: Set Environment Variables (Mac/Linux)
```bash
# Temporary (current session only)
export OpenAI__ApiKey="sk-proj-your-key"
export OpenAI__Model="text-embedding-3-small"
export OpenAI__Endpoint="https://api.openai.com/v1"

# Permanent (add to ~/.bashrc or ~/.zshrc)
echo 'export OpenAI__ApiKey="sk-proj-your-key"' >> ~/.bashrc
source ~/.bashrc
```

#### Step 3: Access in Code
```csharp
// In Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Environment variables are automatically loaded
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
```

#### Step 4: Docker Setup
```yaml
# docker-compose.yml
services:
  web:
    image: eshopwebmvc
    environment:
      - OpenAI__ApiKey=sk-proj-your-key
      - OpenAI__Model=text-embedding-3-small
      - OpenAI__Endpoint=https://api.openai.com/v1
      - AuthorizationConstants__JWT_SECRET_KEY=your-secret-key
      - AuthorizationConstants__AUTH_KEY=your-auth-key
```

---

## Option 3: .env File with DotNetEnv (Development)

### What It Is
Store secrets in a `.env` file that's loaded at runtime.

### Pros ✅
- Easy to manage multiple secrets
- Can be shared with team (with caution)
- Works locally and in Docker
- Clear organization

### Cons ❌
- Must add `.env` to `.gitignore`
- Risk of accidental commit
- Not ideal for production

### Setup Steps

#### Step 1: Install DotNetEnv Package
Add to `Directory.Packages.props`:
```xml
<PackageVersion Include="DotNetEnv" Version="3.1.1" />
```

Add to `src/Web/Web.csproj`:
```xml
<ItemGroup>
    <PackageReference Include="DotNetEnv" />
</ItemGroup>
```

#### Step 2: Create .env File
Create `d:\ClearPointTraining\eShopOnWeb\.env`:
```
# OpenAI Configuration
OpenAI__ApiKey=sk-proj-your-actual-key-here
OpenAI__Model=text-embedding-3-small
OpenAI__Endpoint=https://api.openai.com/v1

# Authorization
AuthorizationConstants__JWT_SECRET_KEY=your-secret-key-minimum-32-characters
AuthorizationConstants__AUTH_KEY=your-auth-key-minimum-32-characters

# Database
ConnectionStrings__CatalogConnection=Server=.;Database=eShopOnWeb;Trusted_Connection=true;
ConnectionStrings__Identity=Server=.;Database=eShopOnWeb_Identity;Trusted_Connection=true;
```

#### Step 3: Add to .gitignore
```bash
# .gitignore
.env
.env.local
.env.*.local
*.env
```

#### Step 4: Load in Program.cs
```csharp
// In src/Web/Program.cs (at the very beginning)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configuration now includes environment variables from .env
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

var openAiKey = builder.Configuration["OpenAI:ApiKey"];
```

#### Step 5: Docker Setup
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY .env .
COPY . .

ENTRYPOINT ["dotnet", "Web.dll"]
```

---

## Option 4: appsettings.{Environment}.json (Development Only)

### What It Is
Environment-specific configuration files.

### Pros ✅
- Built into ASP.NET Core
- Easy to manage per environment
- Can override base settings

### Cons ❌
- Secrets in files (risky)
- Must add to `.gitignore`
- Not recommended for production

### Setup Steps

#### Step 1: Create Environment-Specific Files
```
src/Web/
├── appsettings.json (base)
├── appsettings.Development.json (dev secrets)
└── appsettings.Production.json (prod config)
```

#### Step 2: Add Secrets to Development File
```json
// appsettings.Development.json
{
  "OpenAI": {
    "ApiKey": "sk-proj-your-key",
    "Model": "text-embedding-3-small",
    "Endpoint": "https://api.openai.com/v1"
  },
  "AuthorizationConstants": {
    "JWT_SECRET_KEY": "your-secret-key",
    "AUTH_KEY": "your-auth-key"
  },
  "ConnectionStrings": {
    "CatalogConnection": "Server=.;Database=eShopOnWeb;Trusted_Connection=true;",
    "Identity": "Server=.;Database=eShopOnWeb_Identity;Trusted_Connection=true;"
  }
}
```

#### Step 3: Add to .gitignore
```bash
appsettings.Development.json
appsettings.Production.json
```

#### Step 4: Access in Code
```csharp
// Automatically loaded based on ASPNETCORE_ENVIRONMENT
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
```

---

## Option 5: Vault (Production-Grade, Self-Hosted)

### What It Is
HashiCorp Vault - open-source secrets management system.

### Pros ✅
- Enterprise-grade security
- Self-hosted (no cloud dependency)
- Audit logging
- Dynamic secrets
- Encryption at rest and in transit

### Cons ❌
- Complex setup
- Requires infrastructure
- Overkill for small projects

### Setup Steps

#### Step 1: Install Vault
```bash
# Download from https://www.vaultproject.io/downloads
# Or use Docker
docker run -d -p 8200:8200 vault:latest
```

#### Step 2: Initialize Vault
```bash
vault operator init
vault operator unseal
vault login
```

#### Step 3: Store Secrets
```bash
vault kv put secret/eshopweb \
  openai_api_key="sk-proj-your-key" \
  jwt_secret_key="your-secret-key" \
  auth_key="your-auth-key"
```

#### Step 4: Install NuGet Package
```xml
<PackageVersion Include="VaultSharp" Version="1.13.0.0" />
```

#### Step 5: Access in Code
```csharp
// In Program.cs
var vaultAddr = "http://localhost:8200";
var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

var authMethod = new TokenAuthMethodInfo(vaultToken);
var vaultClient = new VaultClient(new VaultClientSettings(vaultAddr, authMethod));

var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("eshopweb");
var openAiKey = secret.Data.Data["openai_api_key"].ToString();
```

---

## Option 6: Docker Secrets (Docker Swarm)

### What It Is
Docker's built-in secrets management for Swarm mode.

### Pros ✅
- Built into Docker
- Encrypted at rest
- Automatic rotation
- No external tools

### Cons ❌
- Requires Docker Swarm
- Limited to Docker environments

### Setup Steps

#### Step 1: Create Secrets
```bash
echo "sk-proj-your-key" | docker secret create openai_api_key -
echo "your-secret-key" | docker secret create jwt_secret_key -
```

#### Step 2: Use in docker-compose.yml
```yaml
version: '3.1'

services:
  web:
    image: eshopwebmvc
    secrets:
      - openai_api_key
      - jwt_secret_key
    environment:
      - OpenAI__ApiKey_FILE=/run/secrets/openai_api_key
      - AuthorizationConstants__JWT_SECRET_KEY_FILE=/run/secrets/jwt_secret_key

secrets:
  openai_api_key:
    external: true
  jwt_secret_key:
    external: true
```

#### Step 3: Access in Code
```csharp
// Read from mounted file
var openAiKey = File.ReadAllText("/run/secrets/openai_api_key").Trim();
```

---

## Comparison Table

| Option | Dev | Prod | Setup | Security | Sharing |
|--------|-----|------|-------|----------|---------|
| **User Secrets** | ✅ | ❌ | Easy | Good | No |
| **Environment Variables** | ✅ | ✅ | Easy | Fair | Manual |
| **.env File** | ✅ | ⚠️ | Easy | Fair | Risky |
| **appsettings.json** | ✅ | ❌ | Easy | Poor | No |
| **Vault** | ✅ | ✅ | Hard | Excellent | Yes |
| **Docker Secrets** | ⚠️ | ✅ | Medium | Good | Yes |

---

## Recommended Setup for Your Project

### For Development (Right Now)
**Use User Secrets** (Option 1)

```bash
# Initialize
cd src/Web
dotnet user-secrets init

# Store secrets
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-key"
dotnet user-secrets set "OpenAI:Model" "text-embedding-3-small"
dotnet user-secrets set "OpenAI:Endpoint" "https://api.openai.com/v1"
dotnet user-secrets set "AuthorizationConstants:JWT_SECRET_KEY" "your-secret-key"
dotnet user-secrets set "AuthorizationConstants:AUTH_KEY" "your-auth-key"

# Verify
dotnet user-secrets list
```

### For Docker Development
**Use .env File** (Option 3)

```bash
# Create .env file
cat > .env << EOF
OpenAI__ApiKey=sk-proj-your-key
OpenAI__Model=text-embedding-3-small
OpenAI__Endpoint=https://api.openai.com/v1
AuthorizationConstants__JWT_SECRET_KEY=your-secret-key
AuthorizationConstants__AUTH_KEY=your-auth-key
EOF

# Add to .gitignore
echo ".env" >> .gitignore
```

### For Production (Later)
**Use Environment Variables** (Option 2) or **Vault** (Option 5)

```bash
# Set on production server
export OpenAI__ApiKey="sk-proj-your-key"
export OpenAI__Model="text-embedding-3-small"
export OpenAI__Endpoint="https://api.openai.com/v1"
export AuthorizationConstants__JWT_SECRET_KEY="your-secret-key"
export AuthorizationConstants__AUTH_KEY="your-auth-key"
```

---

## Implementation: Update AuthorizationConstants.cs

### Current (INSECURE)
```csharp
public class AuthorizationConstants
{
    public const string AUTH_KEY = "AuthKeyOfDoomThatMustBeAMinimumNumberOfBytes";
    public const string DEFAULT_PASSWORD = "Pass@word1";
    public const string JWT_SECRET_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";
}
```

### New (SECURE)
```csharp
public class AuthorizationConstants
{
    // These will be set from configuration at startup
    public static string AUTH_KEY { get; set; } = "";
    public static string JWT_SECRET_KEY { get; set; } = "";
    
    // Remove DEFAULT_PASSWORD - use proper user creation flow
}
```

### In Program.cs
```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Load .env file if it exists (for local development)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

// Configuration includes: appsettings.json, environment variables, user secrets
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

// Validate and set secrets
var authKey = builder.Configuration["AuthorizationConstants:AUTH_KEY"] 
    ?? throw new InvalidOperationException("AuthorizationConstants:AUTH_KEY not configured");
var jwtSecret = builder.Configuration["AuthorizationConstants:JWT_SECRET_KEY"] 
    ?? throw new InvalidOperationException("AuthorizationConstants:JWT_SECRET_KEY not configured");

AuthorizationConstants.AUTH_KEY = authKey;
AuthorizationConstants.JWT_SECRET_KEY = jwtSecret;

// Rest of configuration...
var app = builder.Build();
```

---

## Security Best Practices

### DO ✅
- Store secrets outside source code
- Add `.env` and `appsettings.*.json` to `.gitignore`
- Use strong, random secrets (minimum 32 characters)
- Rotate secrets regularly
- Use different secrets for each environment
- Log secret access (not the values)
- Encrypt secrets at rest
- Use HTTPS for secret transmission

### DON'T ❌
- Commit secrets to Git
- Hardcode secrets in source code
- Share secrets in Slack/email
- Use same secret for dev and prod
- Log secret values
- Use weak secrets
- Store secrets in plain text files
- Commit `.env` files

---

## Quick Start (5 Minutes)

### Step 1: Initialize User Secrets
```bash
cd src/Web
dotnet user-secrets init
```

### Step 2: Store Your OpenAI Key
```bash
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-actual-key"
dotnet user-secrets set "OpenAI:Model" "text-embedding-3-small"
dotnet user-secrets set "OpenAI:Endpoint" "https://api.openai.com/v1"
```

### Step 3: Store Authorization Secrets
```bash
dotnet user-secrets set "AuthorizationConstants:JWT_SECRET_KEY" "your-secret-key-minimum-32-chars"
dotnet user-secrets set "AuthorizationConstants:AUTH_KEY" "your-auth-key-minimum-32-chars"
```

### Step 4: Verify
```bash
dotnet user-secrets list
```

### Step 5: Update Code
Update `src/ApplicationCore/Constants/AuthorizationConstants.cs` to use configuration instead of hardcoded values.

---

## Troubleshooting

### User Secrets Not Loading?
```bash
# Check if initialized
dotnet user-secrets list

# Re-initialize if needed
dotnet user-secrets clear
dotnet user-secrets init
```

### Environment Variables Not Working?
```bash
# Verify variable is set
echo $OpenAI__ApiKey  # Mac/Linux
echo %OpenAI__ApiKey%  # Windows

# Set again if needed
export OpenAI__ApiKey="sk-proj-your-key"
```

### .env File Not Loading?
- Verify file is in project root
- Check DotNetEnv is installed
- Verify code loads .env in Program.cs
- Check file permissions

---

## Conclusion

For your current situation (no Azure access):

1. **Development:** Use User Secrets (Option 1) - simplest and safest
2. **Docker:** Use .env file (Option 3) - easy to manage
3. **Production:** Use Environment Variables (Option 2) - standard practice

All options are secure and don't require Azure. Choose based on your deployment method.

**Next Step:** Once you have your OpenAI API key, follow the "Quick Start" section above to store it securely!
