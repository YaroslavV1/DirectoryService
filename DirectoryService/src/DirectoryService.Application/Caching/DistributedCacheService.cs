using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace DirectoryService.Application.Caching;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, DistributedCacheEntryOptions options, Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var valueCache = await GetAsync<T>(key, cancellationToken);

        if (valueCache is not null)
        {
            return valueCache;
        }

        var freshValue = await factory();
        if (freshValue is not null)
            await SetAsync(key, freshValue, options, cancellationToken);

        return freshValue;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string? valueCache = await _cache.GetStringAsync(key, token: cancellationToken);
        return valueCache is null
            ? null
            : JsonSerializer.Deserialize<T>(valueCache);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        string valueCache = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(key, valueCache, options, cancellationToken);

        _cacheKeys.TryAdd(key, true);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);

        _cacheKeys.TryRemove(key, out bool _);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keys = _cacheKeys.Keys
            .Where(k => k.StartsWith(prefix))
            .Select(k => RemoveAsync(k, cancellationToken));

        await Task.WhenAll(keys);
    }
}