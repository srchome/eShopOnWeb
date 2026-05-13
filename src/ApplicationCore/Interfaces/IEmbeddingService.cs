using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Generates vector embeddings from text using an AI model.</summary>
public interface IEmbeddingService
{
    /// <summary>Returns a float vector embedding for the given text.</summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}
