using System.Net;
using System.Text.Json;

namespace QrIntegrationSystems.API.Middlewares
{
    // Bu sınıf, gelen her HTTP isteğini izleyen güvenlik ağıdır
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Boru hattındaki bir sonraki middleware/controller
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        // İstek geldiğinde otomatik çalışan metot
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İstek yoluna devam etsin (Controller'a gitsin)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Eğer Controller'da veya Servislerde beklenmedik bir hata fırlatılırsa burası yakalar!
                _logger.LogError(ex, $"[HATA YAKALANDI]: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        // Hata olduğunda React'e döneceğimiz temiz JSON paketi
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500

            var response = new
            {
                success = false,
                statusCode = context.Response.StatusCode,
                message = "Sunucuda beklenmeyen bir hata oluştu.",
                // Geliştirme aşamasında hatanın asıl nedenini görebilmek için:
                correlationId = context.TraceIdentifier,
                detail = _environment.IsDevelopment() ? exception.Message : null 
            };

            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // React için küçük harfle başlasın (success, statusCode)
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
