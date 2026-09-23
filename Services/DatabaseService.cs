using Npgsql;
using Pgvector;
using Pgvector.Npgsql;

namespace rag_saa.Services;

public class DatabaseService
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseService(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("RagDatabase")
            ?? throw new InvalidOperationException(
                "RagDatabase connection string is missing.");

        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();

        _dataSource = builder.Build();
    }

    public async Task InitializeAsync()
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE EXTENSION IF NOT EXISTS vector;

            CREATE TABLE IF NOT EXISTS document_chunks (
                id BIGSERIAL PRIMARY KEY,
                file_path TEXT NOT NULL,
                section TEXT NOT NULL,
                chunk_index INTEGER NOT NULL,
                content TEXT NOT NULL,
                embedding vector(1536)
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertChunksAsync(
        IEnumerable<DocumentChunk> chunks,
        EmbeddingService embeddings)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();

        await using var deleteCommand = connection.CreateCommand();

        deleteCommand.CommandText = """
            TRUNCATE TABLE document_chunks;
            """;

        await deleteCommand.ExecuteNonQueryAsync();

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
            command.Parameters.AddWithValue("embedding", new Vector(vector));

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<DocumentChunk>> SearchAsync(
        float[] embedding,
        int limit = 5)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT file_path, section, chunk_index, content
            FROM document_chunks
            WHERE embedding IS NOT NULL
            ORDER BY embedding <=> @embedding
            LIMIT @limit;
            """;

        command.Parameters.AddWithValue(
            "embedding",
            new Vector(embedding));

        command.Parameters.AddWithValue("limit", limit);

        var results = new List<DocumentChunk>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new DocumentChunk(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(3),
                reader.GetInt32(2)));
        }

        return results;
    }
}