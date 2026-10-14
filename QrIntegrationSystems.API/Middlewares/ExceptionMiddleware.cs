using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QrIntegrationSystems.Application.DTOs.Auth;

namespace QrIntegrationSystems.API.Middlewares;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            switch (ex)
            {
                case AuthRateLimitException limited:
                    context.Response.StatusCode = 429;
                    context.Response.Headers.RetryAfter = limited.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                    await context.Response.WriteAsJsonAsync(new { code = "IP_LOCKED", retryAfterSeconds = limited.RetryAfterSeconds, message = limited.Message });
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { message = "Bu işlem için yetkiniz bulunmuyor." });
                    break;
                case ValidationException:
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(new { message = ex.Message });
                    break;
                case KeyNotFoundException:
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsJsonAsync(new { message = "Kayıt bulunamadı." });
                    break;
                case BadHttpRequestException badRequest:
                    context.Response.StatusCode = badRequest.StatusCode;
                    await context.Response.WriteAsJsonAsync(new { message = "Geçersiz istek." });
                    break;
                default:
                    logger.LogError(ex, "Request failed {TraceId}", context.TraceIdentifier);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { message = "İşlem tamamlanamadı. Daha sonra tekrar deneyin.", correlationId = context.TraceIdentifier });
                    break;
            }
        }
    }
}
