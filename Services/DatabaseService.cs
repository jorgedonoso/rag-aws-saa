using Npgsql;

namespace rag_saa.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("RagDatabase")
            ?? throw new InvalidOperationException(
                "RagDatabase connection string is missing.");
    }

    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE EXTENSION IF NOT EXISTS vector;

            CREATE TABLE IF NOT EXISTS document_chunks (
                id BIGSERIAL PRIMARY KEY,
                file_path TEXT NOT NULL,
                section TEXT NOT NULL,
                chunk_index INTEGER NOT NULL,
                content TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertChunksAsync(
     IEnumerable<DocumentChunk> chunks,
     EmbeddingService embeddings)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var chunk in chunks)
        {
            var vector = await embeddings.GenerateAsync(chunk.Content);

            await using var command = connection.CreateCommand();

            command.CommandText = """
            INSERT INTO document_chunks
                (file_path, section, chunk_index, content, embedding)
            VALUES
                (@file_path, @section, @chunk_index, @content, @embedding);
            """;

            command.Parameters.AddWithValue("file_path", chunk.FilePath);
            command.Parameters.AddWithValue("section", chunk.Section);
            command.Parameters.AddWithValue("chunk_index", chunk.ChunkIndex);
            command.Parameters.AddWithValue("content", chunk.Content);
            command.Parameters.AddWithValue("embedding", vector);

            await command.ExecuteNonQueryAsync();
        }
    }
}
