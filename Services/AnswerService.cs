using OpenAI.Chat;

namespace rag_saa.Services;

public class AnswerService
{
    private readonly ChatClient _client;

    public AnswerService()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        _client = new ChatClient(
            "gpt-5-mini",
            apiKey);
    }

    public async Task<string> GenerateAsync(
        string question,
        IEnumerable<DocumentChunk> chunks)
    {
        var context = string.Join(
            "\n\n---\n\n",
            chunks.Select(chunk =>
                $"Source: {chunk.FilePath} → {chunk.Section}\n{chunk.Content}"));

        var prompt = $"""
            Answer the question using only the provided context.

            If the answer cannot be found in the context, say:
            "I don't have enough information in the provided documents."

            Context:
            {context}

            Question:
            {question}
            """;

        var response = await _client.CompleteChatAsync(prompt);

        return response.Value.Content[0].Text;
    }
}