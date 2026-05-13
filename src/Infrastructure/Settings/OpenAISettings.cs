namespace Microsoft.eShopWeb.Infrastructure.Settings;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}
