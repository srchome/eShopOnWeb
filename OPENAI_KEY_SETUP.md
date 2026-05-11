# OpenAI API Key Setup Guide

## Quick Answer
**Yes, you can use a `.env` file.** You have two main options:

---

## Option 1: Direct OpenAI API Key (Recommended for Development)

### What You Need
- **OpenAI API Key** from https://platform.openai.com/api-keys
- **No Azure subscription required**
- Works immediately with `.env` file

### Setup Steps

#### Step 1: Get Your OpenAI API Key
1. Go to https://platform.openai.com/api-keys
2. Sign in with your OpenAI account (create one if needed)
3. Click "Create new secret key"
4. Copy the key (you'll only see it once)

#### Step 2: Create `.env` File
Create a `.env` file in the root of your project:

```bash
# .env (in project root)
OPENAI_API_KEY=sk-proj-your-actual-key-here
OPENAI_MODEL=text-embedding-3-small
OPENAI_ENDPOINT=https://api.openai.com/v1
```

#### Step 3: Add to `.gitignore`
Make sure `.env` is in your `.gitignore` so you don't commit secrets:

```bash
# .gitignore
.env
.env.local
*.env
```

#### Step 4: Load in Your Application
In `src/Web/Program.cs` or `src/PublicApi/Program.cs`:

```csharp
// Load .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

// Add to configuration
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

// Access the key
var openAiKey = builder.Configuration["OPENAI_API_KEY"];
```

#### Step 5: Install DotNetEnv NuGet Package
Add to `Directory.Packages.props`:

```xml
<PackageVersion Include="DotNetEnv" Version="3.1.1" />
```

Then in your project file (`.csproj`):

```xml
<ItemGroup>
    <PackageReference Include="DotNetEnv" />
</ItemGroup>
```

### Pros & Cons

✅ **Pros:**
- Simple setup, no Azure required
- Works immediately for development
- Direct OpenAI API access
- Lower cost for small-scale testing
- Perfect for local development

❌ **Cons:**
- Not recommended for production
- Secrets stored in file (risky if committed)
- No enterprise support
- Limited to OpenAI models only

### Cost
- **Pay-as-you-go** - Only pay for what you use
- Embeddings: ~$0.02 per 1M tokens
- For 10,000 products: ~$0.20 one-time
- Search queries: ~$0.0001 per query

---

## Option 2: Azure OpenAI (Recommended for Production)

### What You Need
- **Azure subscription** (free tier available)
- **Azure OpenAI resource** deployed
- **No direct OpenAI account needed**

### Setup Steps

#### Step 1: Create Azure OpenAI Resource
```bash
# Using Azure CLI
az cognitiveservices account create \
  --name my-openai-resource \
  --resource-group my-resource-group \
  --kind OpenAI \
  --sku s0 \
  --location eastus
```

Or use Azure Portal:
1. Go to https://portal.azure.com
2. Create new resource → "Azure OpenAI"
3. Fill in details and deploy

#### Step 2: Deploy Model
In Azure Portal:
1. Go to your OpenAI resource
2. Click "Model deployments"
3. Deploy `text-embedding-3-small` model

#### Step 3: Get Connection Details
From Azure Portal:
- **Endpoint**: https://your-resource.openai.azure.com/
- **API Key**: Found in "Keys and Endpoint" section
- **Deployment Name**: Name you gave the model

#### Step 4: Create `.env` File
```bash
# .env (for Azure OpenAI)
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-azure-key-here
AZURE_OPENAI_DEPLOYMENT_NAME=text-embedding-3-small
AZURE_OPENAI_API_VERSION=2024-02-15-preview
```

#### Step 5: Update Application Code
```csharp
// For Azure OpenAI
var client = new AzureOpenAIClient(
    new Uri(builder.Configuration["AZURE_OPENAI_ENDPOINT"]),
    new AzureKeyCredential(builder.Configuration["AZURE_OPENAI_API_KEY"])
);
```

### Pros & Cons

✅ **Pros:**
- Enterprise-grade security
- Integrates with Azure ecosystem
- Better for production
- Can use Azure Key Vault for secrets
- Enterprise support available
- Compliance certifications (SOC 2, ISO 27001, etc.)

❌ **Cons:**
- Requires Azure subscription
- More complex setup
- Slightly higher cost
- Overkill for simple development

### Cost
- **Commitment-based pricing** or **pay-as-you-go**
- Similar pricing to direct OpenAI
- Free tier available for testing

---

## Comparison Table

| Feature | Direct OpenAI | Azure OpenAI |
|---------|---------------|--------------|
| **Setup Complexity** | ⭐ Simple | ⭐⭐⭐ Complex |
| **Development** | ✅ Perfect | ✅ Good |
| **Production** | ⚠️ Not recommended | ✅ Recommended |
| **Security** | ⚠️ Basic | ✅ Enterprise |
| **Cost** | 💰 Lower | 💰 Similar |
| **Azure Integration** | ❌ No | ✅ Yes |
| **Key Vault Support** | ❌ No | ✅ Yes |
| **Enterprise Support** | ❌ No | ✅ Yes |
| **Setup Time** | 5 minutes | 30 minutes |

---

## Recommended Approach for Your Project

### For Development (Right Now)
**Use Direct OpenAI API with `.env` file**

1. Get OpenAI API key from https://platform.openai.com/api-keys
2. Create `.env` file in project root
3. Add to `.gitignore`
4. Load in `Program.cs`
5. Start building!

### For Production (Later)
**Migrate to Azure OpenAI**

1. Set up Azure subscription
2. Create Azure OpenAI resource
3. Deploy model
4. Update configuration
5. Store key in Azure Key Vault

---

## Step-by-Step: Get Started NOW with Direct OpenAI

### 1. Get Your API Key (2 minutes)
```
1. Go to https://platform.openai.com/api-keys
2. Click "Create new secret key"
3. Copy the key (starts with "sk-proj-")
4. Save it somewhere safe
```

### 2. Create `.env` File
In your project root (`d:\ClearPointTraining\eShopOnWeb\.env`):

```
OPENAI_API_KEY=sk-proj-your-key-here
OPENAI_MODEL=text-embedding-3-small
OPENAI_ENDPOINT=https://api.openai.com/v1
```

### 3. Update `.gitignore`
Add to `d:\ClearPointTraining\eShopOnWeb\.gitignore`:

```
# Environment variables
.env
.env.local
.env.*.local
```

### 4. Install NuGet Package
Add to `Directory.Packages.props`:

```xml
<PackageVersion Include="DotNetEnv" Version="3.1.1" />
```

### 5. Update Program.cs
In `src/Web/Program.cs` (after `var builder = WebApplicationBuilder.CreateBuilder(args);`):

```csharp
// Load .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

// Configuration already includes environment variables
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();
```

### 6. Access in Your Code
```csharp
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");
```

---

## Testing Your Setup

### Quick Test
```csharp
// In a test or startup method
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("OPENAI_API_KEY not found in environment");
}
Console.WriteLine("✅ OpenAI API Key loaded successfully");
```

---

## Troubleshooting

### `.env` file not loading?
- Make sure file is in project root (same level as `.sln`)
- Check file name is exactly `.env` (no extension)
- Verify `DotNetEnv` package is installed
- Restart your application

### "API key not found" error?
- Check `.env` file exists and has correct key
- Verify key starts with `sk-proj-`
- Make sure no extra spaces in `.env` file
- Check `.env` is not in `.gitignore` (it should be!)

### "Invalid API key" error?
- Verify you copied the full key from OpenAI
- Check for extra spaces or line breaks
- Try regenerating the key in OpenAI dashboard

### Docker/Container issues?
- Copy `.env` file into container
- Or use environment variables in docker-compose.yml:
  ```yaml
  environment:
    - OPENAI_API_KEY=${OPENAI_API_KEY}
  ```

---

## Security Best Practices

### DO ✅
- Store key in `.env` file (development only)
- Add `.env` to `.gitignore`
- Use Azure Key Vault for production
- Rotate keys regularly
- Use environment variables in CI/CD
- Log API usage for monitoring

### DON'T ❌
- Commit `.env` file to Git
- Hardcode API key in source code
- Share API key in Slack/email
- Use same key for dev and prod
- Log the full API key
- Use in client-side code

---

## Next Steps

1. **Get OpenAI API Key** - https://platform.openai.com/api-keys
2. **Create `.env` file** - Add to project root
3. **Install DotNetEnv** - Add to Directory.Packages.props
4. **Update Program.cs** - Load .env file
5. **Test** - Verify key loads correctly
6. **Start Building** - Implement embedding service

---

## Questions?

- **OpenAI Docs**: https://platform.openai.com/docs
- **Azure OpenAI Docs**: https://learn.microsoft.com/en-us/azure/ai-services/openai/
- **DotNetEnv**: https://github.com/tomlm/dotenv

Ready to proceed? Let me know once you have your OpenAI API key!
