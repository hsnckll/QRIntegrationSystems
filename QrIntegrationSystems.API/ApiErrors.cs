using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QrIntegrationSystems.API;

public static class ApiErrors
{
    public static IActionResult FromException(ControllerBase controller, Exception exception)
    {
        if (exception is UnauthorizedAccessException) return controller.StatusCode(403, new { message = "Bu işlem için yetkiniz bulunmuyor." });
        if (exception is ValidationException) return controller.BadRequest(new { message = exception.Message });
        controller.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("ApiErrors").LogError(exception, "Request failed {TraceId}", controller.HttpContext.TraceIdentifier);
        return controller.StatusCode(500, new { message = "İşlem tamamlanamadı. Daha sonra tekrar deneyin.", correlationId = controller.HttpContext.TraceIdentifier });
    }
}
