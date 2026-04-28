using System.Net;
using System.Net.Mail;

namespace Skolaris.Services
{
  public class EmailService
  {
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
      _config = config;
      _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toNom, string resetLink)
    {
      var smtpHost = _config["EmailSettings:SmtpHost"];
      var smtpPortStr = _config["EmailSettings:SmtpPort"];
      var fromEmail = _config["EmailSettings:FromEmail"];
      var fromPassword = _config["EmailSettings:FromPassword"];
      var fromName = _config["EmailSettings:FromName"] ?? "Skolaris";

      if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
      {
        _logger.LogWarning("Email non configuré. Lien de réinitialisation pour {Email}: {Link}", toEmail, resetLink);
        return false;
      }

      try
      {
        int port = int.TryParse(smtpPortStr, out var p) ? p : 587;

        using var client = new SmtpClient(smtpHost, port);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(fromEmail, fromPassword);

        var message = new MailMessage
        {
          From = new MailAddress(fromEmail, fromName),
          Subject = "Réinitialisation de votre mot de passe — Skolaris",
          IsBodyHtml = true,
          Body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background:#f0f7ff; margin:0; padding:20px;'>
  <div style='max-width:500px; margin:0 auto; background:white; border-radius:12px; padding:35px; box-shadow:0 4px 20px rgba(21,101,192,0.15);'>
    <div style='text-align:center; margin-bottom:25px;'>
      <div style='display:inline-block; background:#1976d2; color:white; border-radius:10px; width:48px; height:48px; line-height:48px; font-size:24px; font-weight:bold;'>S</div>
      <h2 style='color:#1565c0; margin-top:12px;'>Skolaris</h2>
    </div>
    <h3 style='color:#1976d2;'>Bonjour {toNom},</h3>
    <p style='color:#444;'>Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour choisir un nouveau mot de passe.</p>
    <div style='text-align:center; margin:30px 0;'>
      <a href='{resetLink}' style='background:#1976d2; color:white; padding:14px 32px; border-radius:8px; text-decoration:none; font-weight:bold; display:inline-block;'>
        Réinitialiser mon mot de passe
      </a>
    </div>
    <p style='color:#777; font-size:13px;'>Ce lien expire dans 1 heure. Si vous n'avez pas fait cette demande, ignorez cet email.</p>
    <hr style='border:none; border-top:1px solid #e3f2fd; margin:20px 0;'>
    <p style='color:#999; font-size:12px; text-align:center;'>© Skolaris — Plateforme scolaire</p>
  </div>
</body>
</html>"
        };
        message.To.Add(new MailAddress(toEmail, toNom));

        await client.SendMailAsync(message);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erreur lors de l'envoi du courriel à {Email}", toEmail);
        return false;
      }
    }
  }
}
