using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.DTOs.AuditLog;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.API.Controllers;

[ApiController]
[Route("api/log")]
[Authorize(Roles = "SuperAdmin")]
public class LogController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? businessId, CancellationToken cancellationToken)
    {
        var query = db.Logs.AsNoTracking();
        if (businessId.HasValue) query = query.Where(l => l.BusinessId == businessId.Value);
        return Ok(await query.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
            .Select(l => new LogResponseDto { Id = l.Id, BusinessId = l.BusinessId,
                ActorType = l.ActorType, Action = l.Action, EntityType = l.EntityType,
                EntityId = l.EntityId, OldValues = l.OldValues, NewValues = l.NewValues,
                CreatedAt = l.CreatedAt }).ToListAsync(cancellationToken));
    }
}
