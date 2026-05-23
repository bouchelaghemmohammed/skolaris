namespace Skolaris.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
