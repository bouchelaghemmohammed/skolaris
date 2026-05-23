using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class AuditLogService
    {
        // Injection du DbContext
        private readonly ApplicationDbContext _context;

        // Constructeur
        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ajouter un log dans la base de données
        public async Task AddLogAsync(
            string userId,
            string userName,
            string role,
            string action,
            string details,
            string? ipAddress = null)
        {
            // Création du log
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

            // Ajouter dans la table AuditLogs
            _context.AuditLogs.Add(log);

            // Sauvegarder les changements
            await _context.SaveChangesAsync();
        }

        // Retourner tous les logs
        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs

                // Trier du plus récent au plus ancien
                .OrderByDescending(l => l.CreatedAt)

                // Transformer en liste
                .ToListAsync();
        }

        // Retourner les logs d'un utilisateur précis
        public async Task<List<AuditLog>> GetByUserAsync(string userId)
        {
            return await _context.AuditLogs

                // Filtrer selon utilisateur
                .Where(l => l.UserId == userId)

                // Trier par date
                .OrderByDescending(l => l.CreatedAt)

                // Retourner liste
                .ToListAsync();
        }

        // Supprimer un log
        public async Task DeleteLogAsync(int id)
        {
            // Chercher le log
            var log = await _context.AuditLogs.FindAsync(id);

            // Vérifier si existe
            if (log != null)
            {
                // Supprimer
                _context.AuditLogs.Remove(log);

                // Sauvegarder
                await _context.SaveChangesAsync();
            }
        }

        // Supprimer tous les logs
        public async Task ClearLogsAsync()
        {
            // Supprimer toute la table
            _context.AuditLogs.RemoveRange(_context.AuditLogs);

            // Sauvegarder
            await _context.SaveChangesAsync();
        }
    }
}