using Microsoft.AspNetCore.Identity;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Utilisateur> _passwordHasher;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Utilisateur>();
            _emailService = emailService;
            _config = config;
        }

        public Utilisateur? Login(string email, string password)
        {
            string emailSaisi = email.Trim().ToLower();

            var user = _context.Utilisateurs.FirstOrDefault(u =>
                u.Email.ToLower() == emailSaisi);

            if (user == null || !user.IsActive)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.MotDePasse, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email, string baseUrl)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u =>
                u.Email.ToLower() == email.Trim().ToLower());

            if (user == null)
                return new ForgotPasswordResult { Success = true };

            var token = Guid.NewGuid().ToString("N");
            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            _context.SaveChanges();

            var resetLink = $"{baseUrl}/reset-password?token={token}";
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email, $"{user.Prenom} {user.Nom}", resetLink);

            return new ForgotPasswordResult
            {
                Success = true,
                EmailSent = emailSent,
                DevToken = emailSent ? null : token
            };
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordTokenExpiry > DateTime.UtcNow);

            if (user == null) return false;

            user.MotDePasse = _passwordHasher.HashPassword(user, newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;
            _context.SaveChanges();

            return true;
        }
    }

    public class ForgotPasswordResult
    {
        public bool Success { get; set; }
        public bool EmailSent { get; set; }
        public string? DevToken { get; set; }
    }
}
