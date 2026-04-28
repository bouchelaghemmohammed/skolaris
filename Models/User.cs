namespace Skolaris.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordTokenExpiry { get; set; }
    }
}