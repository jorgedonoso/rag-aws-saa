namespace rag_saa.Services;

public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(this IServiceCollection services)
    {
        services.AddSingleton<IngestionService>();
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<EmbeddingService>();
        services.AddSingleton<AnswerService>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        var database = services.GetRequiredService<DatabaseService>();
        await database.InitializeAsync();
    }
}