using System.Text.Json;
using System.Text.Json.Serialization;
using TaskHub.Models;

namespace TaskHub.Storage;

public sealed class JsonTaskStorage : IDisposable
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };
    private bool _isDisposed;

    public JsonTaskStorage(string filePath)
    {
        _filePath = filePath;
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task SaveAsync(IEnumerable<TaskItem> tasks)
    {
        ThrowIfDisposed();

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, tasks, _jsonOptions);
    }

    public async Task<IReadOnlyList<TaskItem>> LoadAsync()
    {
        ThrowIfDisposed();

        if (!File.Exists(_filePath))
        {
            return Array.Empty<TaskItem>();
        }

        await using var stream = File.OpenRead(_filePath);
        var tasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream, _jsonOptions);
        return tasks ?? new List<TaskItem>();
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(JsonTaskStorage));
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
    }
}
