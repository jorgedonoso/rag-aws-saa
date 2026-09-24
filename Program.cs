using rag_saa.Services;

var builder = WebApplication.CreateBuilder(args);

// Register application services.
builder.Services.AddRagServices();

var app = builder.Build();

// Initialize the database startup task
await app.Services.InitializeDatabaseAsync();

// Register API endpoints.
app.MapRagEndpoints();

app.Run();