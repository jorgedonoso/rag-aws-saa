using rag_saa.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IngestionService>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<EmbeddingService>();

var app = builder.Build();

var database = app.Services.GetRequiredService<DatabaseService>();
await database.InitializeAsync();

app.MapGet("/", () => "RAG API");

app.MapGet("/ingest", async (
    IngestionService ingestion,
    DatabaseService database,
    EmbeddingService embeddings) =>
{
    var chunks = ingestion.Ingest();

    await database.InsertChunksAsync(chunks, embeddings);

    return Results.Ok(new
    {
        ChunkCount = chunks.Count
    });
});

app.Run();
