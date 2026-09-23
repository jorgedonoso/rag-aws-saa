using rag_saa.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IngestionService>();
builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();

var database = app.Services.GetRequiredService<DatabaseService>();
await database.InitializeAsync();

app.MapGet("/", () => "RAG API");

app.MapGet("/ingest", async (
    IngestionService ingestion,
    DatabaseService database) =>
{
    var chunks = ingestion.Ingest();

    await database.InsertChunksAsync(chunks);

    return Results.Ok(new
    {
        ChunkCount = chunks.Count,
        Chunks = chunks.Take(5)
    });
});

app.Run();
