using OpenAI.Embeddings;

namespace rag_saa.Services;

public class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        _client = new EmbeddingClient(
            "text-embedding-3-small",
            apiKey);
    }

    public async Task<float[]> GenerateAsync(string text)
    {
        var result = await _client.GenerateEmbeddingAsync(text);

        return result.Value.ToFloats().ToArray();
    }
}