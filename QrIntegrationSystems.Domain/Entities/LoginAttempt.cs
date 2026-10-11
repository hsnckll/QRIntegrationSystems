using System;

namespace QrIntegrationSystems.Domain.Entities
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string IpAddress { get; set; } = null!;
        public int FailedCount { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }
        public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;
    }
}