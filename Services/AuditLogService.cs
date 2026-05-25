using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL
        // =========================================================
        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // =========================================================
        // ADD LOG
        // =========================================================
        public async Task AddLogAsync(
            string userId,
            string userName,
            string role,
            string action,
            string details,
            string? ipAddress = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Role = role,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // DELETE ONE
        // =========================================================
        public async Task DeleteLogAsync(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);

            if (log == null)
                return;

            _context.AuditLogs.Remove(log);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // CLEAR ALL
        // =========================================================
        public async Task ClearLogsAsync()
        {
            var logs = await _context.AuditLogs.ToListAsync();

            _context.AuditLogs.RemoveRange(logs);

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // AUTH LOGS
        // =========================================================
        public async Task LogConnexionAsync(
            Utilisateur user,
            string? ip = null)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Connexion",
                $"Connexion réussie pour {user.Email}",
                ip
            );
        }

        public async Task LogLogoutAsync(
            Utilisateur user,
            string? ip = null)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Déconnexion",
                $"Déconnexion utilisateur {user.Email}",
                ip
            );
        }

        public async Task LogPasswordResetAsync(
            Utilisateur user,
            string? ip = null)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Réinitialisation mot de passe",
                $"Mot de passe modifié pour {user.Email}",
                ip
            );
        }

        // =========================================================
        // HORAIRES
        // =========================================================
        public async Task LogCreateHoraireAsync(
            Utilisateur user,
            string cours,
            string groupe)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Création horaire",
                $"Horaire créé : {cours} - Groupe {groupe}"
            );
        }

        public async Task LogUpdateHoraireAsync(
            Utilisateur user,
            string cours,
            string groupe)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Modification horaire",
                $"Horaire modifié : {cours} - Groupe {groupe}"
            );
        }

        public async Task LogDeleteHoraireAsync(
            Utilisateur user,
            string cours)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Suppression horaire",
                $"Horaire supprimé : {cours}"
            );
        }

        public async Task LogPublishHoraireAsync(
            Utilisateur user,
            string cours)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Publication horaire",
                $"Horaire publié : {cours}"
            );
        }

        // =========================================================
        // ABSENCES
        // =========================================================
        public async Task LogCreateAbsenceAsync(
            Utilisateur user,
            int eleveId)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Création absence",
                $"Absence ajoutée pour élève #{eleveId}"
            );
        }

        public async Task LogUpdateAbsenceAsync(
            Utilisateur user,
            int absenceId)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Modification absence",
                $"Absence modifiée #{absenceId}"
            );
        }

        public async Task LogDeleteAbsenceAsync(
            Utilisateur user,
            int absenceId)
        {
            await AddLogAsync(
                user.IdUtilisateur.ToString(),
                $"{user.Prenom} {user.Nom}",
                user.Role.ToString(),
                "Suppression absence",
                $"Absence supprimée #{absenceId}"
            );
        }

        // =========================================================
        // USERS
        // =========================================================
        public async Task LogCreateUserAsync(
            Utilisateur admin,
            Utilisateur createdUser)
        {
            await AddLogAsync(
                admin.IdUtilisateur.ToString(),
                $"{admin.Prenom} {admin.Nom}",
                admin.Role.ToString(),
                "Création utilisateur",
                $"Utilisateur créé : {createdUser.Email}"
            );
        }

        public async Task LogRoleChangeAsync(
            Utilisateur admin,
            Utilisateur targetUser,
            string newRole)
        {
            await AddLogAsync(
                admin.IdUtilisateur.ToString(),
                $"{admin.Prenom} {admin.Nom}",
                admin.Role.ToString(),
                "Changement rôle",
                $"{targetUser.Email} → {newRole}"
            );
        }

        public async Task LogActivationAsync(
            Utilisateur admin,
            Utilisateur targetUser)
        {
            await AddLogAsync(
                admin.IdUtilisateur.ToString(),
                $"{admin.Prenom} {admin.Nom}",
                admin.Role.ToString(),
                "Activation compte",
                $"Compte activé : {targetUser.Email}"
            );
        }

        public async Task LogDesactivationAsync(
            Utilisateur admin,
            Utilisateur targetUser)
        {
            await AddLogAsync(
                admin.IdUtilisateur.ToString(),
                $"{admin.Prenom} {admin.Nom}",
                admin.Role.ToString(),
                "Désactivation compte",
                $"Compte désactivé : {targetUser.Email}"
            );
        }
    }
}