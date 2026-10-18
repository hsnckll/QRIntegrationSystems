namespace QrIntegrationSystems.Application.Interfaces;

public interface IBusinessScopeResolver
{
    int OwnBusinessId();
    int ResolveSelectedBusiness();
    int? ResolveUpload(int? selected);
    void EnsureAccess(int businessId);
}
