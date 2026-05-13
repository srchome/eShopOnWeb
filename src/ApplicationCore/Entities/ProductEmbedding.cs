using System;
using System.Text.Json;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;

public class ProductEmbedding : BaseEntity, IAggregateRoot
{
    public int CatalogItemId { get; private set; }
    public string EmbeddingJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private ProductEmbedding() { }

    public ProductEmbedding(int catalogItemId, float[] embedding)
    {
        CatalogItemId = catalogItemId;
        EmbeddingJson = JsonSerializer.Serialize(embedding);
        CreatedAt = DateTime.UtcNow;
    }

    public float[] GetEmbedding() => JsonSerializer.Deserialize<float[]>(EmbeddingJson) ?? [];
}
