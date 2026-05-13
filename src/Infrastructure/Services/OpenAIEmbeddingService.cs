using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIEmbeddingService> _logger;
    private EmbeddingClient? _client;

    public OpenAIEmbeddingService(IOptions<OpenAISettings> settings, ILogger<OpenAIEmbeddingService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        _client ??= CreateClient();
        _logger.LogInformation("Generating embedding for text of length {Length}", text.Length);
        var result = await _client.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return result.Value.ToFloats().ToArray();
    }

    private EmbeddingClient CreateClient()
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Set OpenAI__ApiKey in your environment.");
        return new OpenAIClient(_settings.ApiKey).GetEmbeddingClient(_settings.EmbeddingModel);
    }
}
