using QrIntegrationSystems.Application.Interfaces;

namespace QrIntegrationSystems.API;

public sealed class BusinessScopeResolver(IHttpContextAccessor accessor) : IBusinessScopeResolver
{
    private HttpContext Context => accessor.HttpContext ?? throw new UnauthorizedAccessException();
    public int OwnBusinessId()
    {
        if (Context.User.IsInRole("Business") && int.TryParse(Context.User.FindFirst("BusinessId")?.Value, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException();
    }
    public int ResolveSelectedBusiness()
    {
        if (!Context.User.IsInRole("SuperAdmin")) return OwnBusinessId();
        if (int.TryParse(Context.Request.Query["businessId"], out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("İşletme seçimi gerekli.");
    }
    public int? ResolveUpload(int? selected)
    {
        if (!Context.User.IsInRole("SuperAdmin")) return OwnBusinessId();
        if (selected.HasValue && selected.Value <= 0) throw new System.ComponentModel.DataAnnotations.ValidationException("Geçerli bir işletme seçin.");
        return selected;
    }
    public void EnsureAccess(int businessId)
    {
        if (!Context.User.IsInRole("SuperAdmin") && OwnBusinessId() != businessId) throw new UnauthorizedAccessException();
    }
}
