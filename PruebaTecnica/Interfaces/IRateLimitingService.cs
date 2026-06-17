namespace PruebaTecnica.Interfaces;

public interface IRateLimitingService
{
    /// <summary>
    /// Checks if the provider has exceeded the rate limit.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>True if the request is allowed (within limit), False if exceeded.</returns>
    bool IsRequestAllowed(string providerId);
}
