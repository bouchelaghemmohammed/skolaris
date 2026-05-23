using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class EmploiDuTempsService
    {
        private readonly ApplicationDbContext _context;

        public EmploiDuTempsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retourne tous les créneaux
        public async Task<List<EmploiDuTemps>> GetAllAsync()
        {
            return await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Enseignant)
                        .ThenInclude(ens => ens!.Utilisateur)
                .OrderBy(e => e.JourSemaine)
                .ThenBy(e => e.HeureDebut)
                .ToListAsync();
        }

        // Retourne un créneau par ID
        public async Task<EmploiDuTemps?> GetByIdAsync(int id)
        {
            return await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                .FirstOrDefaultAsync(e => e.IdEmploi == id);
        }

        // Ajouter un créneau
        public async Task<(bool Success, string Message)> CreateAsync(EmploiDuTempsCreateDto dto)
        {
            // Vérifie jour valide
            if (!Enum.IsDefined(typeof(JourSemaine), dto.JourSemaine))
            {
                return (false, "Jour invalide.");
            }

            // Vérifie cours offert
            var coursOffert = await _context.CoursOfferts
                .FirstOrDefaultAsync(c => c.IdCoursOffert == dto.CoursOffertId);

            if (coursOffert == null)
            {
                return (false, "Cours offert introuvable.");
            }

            // Vérifie format heure
            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
            {
                return (false, "Heure début invalide.");
            }

            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
            {
                return (false, "Heure fin invalide.");
            }

            // Vérifie ordre des heures
            if (debut >= fin)
            {
                return (false, "Heure début doit être avant heure fin.");
            }

            var jour = (JourSemaine)dto.JourSemaine;

            // Détection conflits
            bool conflit = await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                .AnyAsync(e =>
                    e.JourSemaine == jour &&
                    debut < e.HeureFin &&
                    fin > e.HeureDebut &&
                    (
                        e.Salle == dto.Salle ||
                        e.CoursOffert.IdEnseignant == coursOffert.IdEnseignant ||
                        e.CoursOffert.IdGroupe == coursOffert.IdGroupe
                    )
                );

            if (conflit)
            {
                return (false, "Conflit d'horaire détecté.");
            }

            var emploi = new EmploiDuTemps
            {
                IdCoursOffert = dto.CoursOffertId,
                JourSemaine = jour,
                HeureDebut = debut,
                HeureFin = fin,
                Salle = dto.Salle,
                IsPublie = false
            };

            _context.EmploisDuTemps.Add(emploi);
            await _context.SaveChangesAsync();

            return (true, "Créneau ajouté avec succès.");
        }

        // Modifier un créneau
        public async Task<(bool Success, string Message)> UpdateAsync(int id, EmploiDuTempsCreateDto dto)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
            {
                return (false, "Créneau introuvable.");
            }

            var coursOffert = await _context.CoursOfferts
                .FirstOrDefaultAsync(c => c.IdCoursOffert == dto.CoursOffertId);

            if (coursOffert == null)
            {
                return (false, "Cours offert introuvable.");
            }

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
            {
                return (false, "Heure début invalide.");
            }

            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
            {
                return (false, "Heure fin invalide.");
            }

            if (debut >= fin)
            {
                return (false, "Heure début doit être avant heure fin.");
            }

            var jour = (JourSemaine)dto.JourSemaine;

            bool conflit = await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                .AnyAsync(e =>
                    e.IdEmploi != id &&
                    e.JourSemaine == jour &&
                    debut < e.HeureFin &&
                    fin > e.HeureDebut &&
                    (
                        e.Salle == dto.Salle ||
                        e.CoursOffert.IdEnseignant == coursOffert.IdEnseignant ||
                        e.CoursOffert.IdGroupe == coursOffert.IdGroupe
                    )
                );

            if (conflit)
            {
                return (false, "Conflit d'horaire détecté.");
            }

            emploi.IdCoursOffert = dto.CoursOffertId;
            emploi.JourSemaine = jour;
            emploi.HeureDebut = debut;
            emploi.HeureFin = fin;
            emploi.Salle = dto.Salle;

            await _context.SaveChangesAsync();

            return (true, "Créneau modifié avec succès.");
        }

        // Supprimer
        public async Task<bool> DeleteAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
            {
                return false;
            }

            _context.EmploisDuTemps.Remove(emploi);
            await _context.SaveChangesAsync();

            return true;
        }

        // Publier
        public async Task<bool> PublierAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
            {
                return false;
            }

            emploi.IsPublie = true;

            await _context.SaveChangesAsync();

            return true;
        }

        // Dépublier
        public async Task<bool> DepublierAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
            {
                return false;
            }

            emploi.IsPublie = false;

            await _context.SaveChangesAsync();

            return true;
        }
    }

    // DTO
    public class EmploiDuTempsCreateDto
    {
        public int CoursOffertId { get; set; }

        public int JourSemaine { get; set; }

        public string HeureDebut { get; set; } = "";

        public string HeureFin { get; set; } = "";

        public string? Salle { get; set; }
    }
}