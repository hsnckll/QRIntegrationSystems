using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace QrIntegrationSystems.Infrastructure.Services;

public sealed class AuthSessionStore
{
    private readonly string root;
    private readonly object gate = new();
    private readonly SemaphoreSlim[] locks = Enumerable.Range(0, 1024).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    public AuthSessionStore(IConfiguration config) => root = Path.GetFullPath(config["Storage:AuthPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage", "auth"));
    public async Task<IDisposable> AcquireAsync(string key)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        var group = key.StartsWith("attempt:") ? 1 : key.StartsWith("upload:") ? 2 : key.StartsWith("scan:") ? 3 : 0;
        var semaphore = locks[group * 256 + hash[0]];
        await semaphore.WaitAsync(); return new Release(semaphore);
    }
    public string BusinessVersion(int id)
    {
        lock (gate) { var file = Path.Combine(root, $"business-{id}.json"); if (!File.Exists(file)) Write(file, Guid.NewGuid().ToString("N")); return JsonSerializer.Deserialize<string>(File.ReadAllText(file))!; }
    }
    public void RevokeBusiness(int id) { lock (gate) Write(Path.Combine(root, $"business-{id}.json"), Guid.NewGuid().ToString("N")); }
    public bool IsRevoked(string jti)
    {
        if (!Guid.TryParseExact(jti, "N", out _)) return true;
        lock (gate) return File.Exists(Path.Combine(root, $"session-{jti}.json"));
    }
    public void Revoke(string jti, DateTime expires)
    {
        if (!Guid.TryParseExact(jti, "N", out _)) return;
        lock (gate)
        {
            Write(Path.Combine(root, $"session-{jti}.json"), expires);
            foreach (var file in Directory.EnumerateFiles(root, "session-*.json"))
                if (JsonSerializer.Deserialize<DateTime>(File.ReadAllText(file)) < DateTime.UtcNow) File.Delete(file);
        }
    }
    private void Write<T>(string path, T value)
    {
        Directory.CreateDirectory(root); var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        File.WriteAllText(temporary, JsonSerializer.Serialize(value)); File.Move(temporary, path, true);
    }
    private sealed class Release(SemaphoreSlim semaphore) : IDisposable { public void Dispose() => semaphore.Release(); }
}
