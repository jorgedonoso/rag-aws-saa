using rag_saa.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IngestionService>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<AnswerService>();

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

app.MapGet("/ask", async (
    string q,
    EmbeddingService embeddings,
    DatabaseService database,
    AnswerService answer) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Query is required.");

    var queryEmbedding = await embeddings.GenerateAsync(q);

    var chunks = await database.SearchAsync(queryEmbedding);

    var response = await answer.GenerateAsync(q, chunks);

    return Results.Ok(new
    {
        Question = q,
        Answer = response,
        Sources = chunks.Select(x => new
        {
            x.FilePath,
            x.Section,
            x.ChunkIndex
        })
    });
});

app.Run();
