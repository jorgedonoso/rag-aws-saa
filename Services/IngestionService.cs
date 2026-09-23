using System.Text.RegularExpressions;
using System.Text;

namespace rag_saa.Services;

public class IngestionService
{
    private readonly string _documentsPath;

    public IngestionService(IConfiguration configuration)
    {
        _documentsPath = configuration["Rag:DocumentsPath"]
            ?? throw new InvalidOperationException("Rag:DocumentsPath is not configured.");
    }

    public List<DocumentChunk> Ingest()
    {
        var fullPath = Path.GetFullPath(_documentsPath);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException(
                $"Documents directory not found: {fullPath}");

        var files = Directory.GetFiles(
            fullPath,
            "*.md",
            SearchOption.AllDirectories);

        var chunks = new List<DocumentChunk>();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);

            chunks.AddRange(
                ChunkMarkdown(
                    Path.GetRelativePath(fullPath, file),
                    content));
        }

        return chunks;
    }

    private static IEnumerable<DocumentChunk> SplitLargeSection(
        string filePath,
        string section,
        List<string> lines)
    {
        const int maxChunkSize = 1200;

        var text = string.Join('\n', lines).Trim();

        if (text.Length <= maxChunkSize)
        {
            yield return new DocumentChunk(
                filePath,
                section,
                text);

            yield break;
        }

        var paragraphs = text.Split(
            "\n\n",
            StringSplitOptions.RemoveEmptyEntries);

        var current = new StringBuilder();
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if (current.Length > 0 &&
                current.Length + paragraph.Length + 2 > maxChunkSize)
            {
                yield return new DocumentChunk(
                    filePath,
                    section,
                    current.ToString().Trim(),
                    chunkIndex++);

                current.Clear();
            }

            if (current.Length > 0)
                current.Append("\n\n");

            current.Append(paragraph);
        }

        if (current.Length > 0)
        {
            yield return new DocumentChunk(
                filePath,
                section,
                current.ToString().Trim(),
                chunkIndex);
        }
    }

    private static IEnumerable<DocumentChunk> ChunkMarkdown(
    string filePath,
    string content)
    {
        var lines = content.Split('\n');

        var currentSection = "Introduction";
        var currentContent = new List<string>();

        foreach (var line in lines)
        {
            var heading = Regex.Match(line, @"^(#{1,6})\s+(.+)$");

            if (heading.Success && currentContent.Count > 0)
            {
                foreach (var chunk in SplitLargeSection(
                    filePath,
                    currentSection,
                    currentContent))
                {
                    yield return chunk;
                }

                currentContent.Clear();
                currentSection = heading.Groups[2].Value.Trim();
            }

            currentContent.Add(line);
        }

        if (currentContent.Count > 0)
        {
            foreach (var chunk in SplitLargeSection(
                filePath,
                currentSection,
                currentContent))
            {
                yield return chunk;
            }
        }
    }
    private static DocumentChunk CreateChunk(
        string filePath,
        string section,
        List<string> lines)
    {
        return new DocumentChunk(
            filePath,
            section,
            string.Join('\n', lines).Trim());
    }
}

public record DocumentChunk(
    string FilePath,
    string Section,
    string Content,
    int ChunkIndex = 0);
