using OpenAI.Embeddings;

namespace rag_saa.Services;

public class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
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