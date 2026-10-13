namespace QrIntegrationSystems.Application.DTOs.Auth;

public sealed class AuthRateLimitException(int retryAfterSeconds) : Exception("Çok fazla deneme yaptınız. Daha sonra tekrar deneyin.")
{
    public int RetryAfterSeconds { get; } = Math.Max(1, retryAfterSeconds);
}
