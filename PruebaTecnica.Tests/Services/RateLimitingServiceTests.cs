using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using PruebaTecnica.Services;
using Xunit;

namespace PruebaTecnica.Tests.Services;

public class RateLimitingServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public RateLimitingServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var inMemorySettings = new System.Collections.Generic.Dictionary<string, string> {
            {"RateLimiting:MaxRequestsPerMinute", "10"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public void IsRequestAllowed_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var service = new RateLimitingService(_memoryCache, _configuration);
        var providerId = "P1";

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            var result = service.IsRequestAllowed(providerId);
            Assert.True(result, $"Request {i + 1} should be allowed.");
        }
    }

    [Fact]
    public void IsRequestAllowed_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var service = new RateLimitingService(_memoryCache, _configuration);
        var providerId = "P2";

        // Act
        for (int i = 0; i < 10; i++)
        {
            service.IsRequestAllowed(providerId);
        }
        
        // The 11th request should be blocked
        var result = service.IsRequestAllowed(providerId);

        // Assert
        Assert.False(result, "Request 11 should be blocked by rate limit.");
    }
    
    [Fact]
    public void IsRequestAllowed_EmptyProviderId_ReturnsFalse()
    {
        // Arrange
        var service = new RateLimitingService(_memoryCache, _configuration);

        // Act
        var result = service.IsRequestAllowed("");

        // Assert
        Assert.False(result);
    }
}
