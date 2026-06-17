using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PruebaTecnica.Interfaces;

namespace PruebaTecnica.Services;

public class RateLimitingService : IRateLimitingService
{
    private readonly IMemoryCache _memoryCache;
    private readonly int _maxRequestsPerMinute;

    public RateLimitingService(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        // Default to 10 requests per minute as per requirements
        _maxRequestsPerMinute = configuration.GetValue<int>("RateLimiting:MaxRequestsPerMinute", 10);
    }

    public bool IsRequestAllowed(string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            // If provider ID is empty, we don't allow the request (or could be handled differently based on exact business logic)
            return false;
        }

        string cacheKey = $"RateLimit_{providerId}";

        // We use a counter that expires 1 minute after the FIRST request in the window.
        // There are multiple algorithms (Leaky Bucket, Token Bucket, Fixed Window, Sliding Window).
        // For simplicity and efficiency, we implement a Fixed Window approach using IMemoryCache.
        
        // Try to get current count
        if (_memoryCache.TryGetValue(cacheKey, out int currentCount))
        {
            if (currentCount >= _maxRequestsPerMinute)
            {
                // Limit exceeded
                return false;
            }

            // Increment the count. The expiration time remains the same since we update the value.
            _memoryCache.Set(cacheKey, currentCount + 1, GetCacheEntryOptions());
            return true;
        }

        // First request for this provider in the new window
        _memoryCache.Set(cacheKey, 1, GetCacheEntryOptions());
        return true;
    }

    private MemoryCacheEntryOptions GetCacheEntryOptions()
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };
    }
}
