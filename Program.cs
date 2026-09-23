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

app.MapGet("/search", async (
    string q,
    EmbeddingService embeddings,
    DatabaseService database) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Query is required.");

    var queryEmbedding = await embeddings.GenerateAsync(q);

    var results = await database.SearchAsync(queryEmbedding);

    return Results.Ok(new
    {
        Query = q,
        Results = results
    });
});

app.Run();
