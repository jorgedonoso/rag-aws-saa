using rag_saa.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IngestionService>();

var app = builder.Build();
app.MapGet("/", () => "RAG API");

app.MapGet("/ingest", (IngestionService ingestion) =>
{
    var chunks = ingestion.Ingest();

    return Results.Ok(new
    {
        ChunkCount = chunks.Count,
        Chunks = chunks.Take(5)
    });
});

app.Run();
