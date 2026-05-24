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

        public async Task<List<object>> GetAllAsync()
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
                .Select(e => new
                {
                    id = e.IdEmploi,
                    coursOffertId = e.IdCoursOffert,
                    coursNom = e.CoursOffert.Cours.Nom,
                    groupeId = e.CoursOffert.IdGroupe,
                    groupeNom = e.CoursOffert.Groupe.Nom,
                    enseignantNom =
                        e.CoursOffert.Enseignant.Utilisateur.Prenom + " " +
                        e.CoursOffert.Enseignant.Utilisateur.Nom,
                    jourSemaine = e.JourSemaine.ToString(),
                    heureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin = e.HeureFin.ToString(@"hh\:mm"),
                    salle = e.Salle,
                    isPublie = e.IsPublie
                })
                .ToListAsync<object>();
        }

        public async Task<object?> GetByIdAsync(int id)
        {
            return await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Where(e => e.IdEmploi == id)
                .Select(e => new
                {
                    id = e.IdEmploi,
                    coursOffertId = e.IdCoursOffert,
                    coursNom = e.CoursOffert.Cours.Nom,
                    groupeId = e.CoursOffert.IdGroupe,
                    groupeNom = e.CoursOffert.Groupe.Nom,
                    jourSemaine = e.JourSemaine.ToString(),
                    heureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin = e.HeureFin.ToString(@"hh\:mm"),
                    salle = e.Salle,
                    isPublie = e.IsPublie
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> CreateAsync(EmploiDuTempsCreateDto dto)
        {
            if (!Enum.IsDefined(typeof(JourSemaine), dto.JourSemaine))
                return (false, "Jour invalide.");

            var coursOffert = await _context.CoursOfferts
                .FirstOrDefaultAsync(c => c.IdCoursOffert == dto.CoursOffertId);

            if (coursOffert == null)
                return (false, "Cours offert introuvable.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return (false, "Heure début invalide.");

            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return (false, "Heure fin invalide.");

            if (debut >= fin)
                return (false, "Heure début doit être avant heure fin.");

            var jour = (JourSemaine)dto.JourSemaine;

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
                return (false, "Conflit d'horaire détecté.");

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

        public async Task<(bool Success, string Message)> UpdateAsync(int id, EmploiDuTempsCreateDto dto)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
                return (false, "Créneau introuvable.");

            if (!Enum.IsDefined(typeof(JourSemaine), dto.JourSemaine))
                return (false, "Jour invalide.");

            var coursOffert = await _context.CoursOfferts
                .FirstOrDefaultAsync(c => c.IdCoursOffert == dto.CoursOffertId);

            if (coursOffert == null)
                return (false, "Cours offert introuvable.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return (false, "Heure début invalide.");

            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return (false, "Heure fin invalide.");

            if (debut >= fin)
                return (false, "Heure début doit être avant heure fin.");

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
                return (false, "Conflit d'horaire détecté.");

            emploi.IdCoursOffert = dto.CoursOffertId;
            emploi.JourSemaine = jour;
            emploi.HeureDebut = debut;
            emploi.HeureFin = fin;
            emploi.Salle = dto.Salle;

            await _context.SaveChangesAsync();

            return (true, "Créneau modifié avec succès.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
                return false;

            _context.EmploisDuTemps.Remove(emploi);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> PublierAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
                return false;

            emploi.IsPublie = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DepublierAsync(int id)
        {
            var emploi = await _context.EmploisDuTemps.FindAsync(id);

            if (emploi == null)
                return false;

            emploi.IsPublie = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task PublierToutAsync()
        {
            var emplois = await _context.EmploisDuTemps.ToListAsync();

            foreach (var e in emplois)
                e.IsPublie = true;

            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetHoraireEleveAsync(int idUtilisateur)
        {
            var eleve = await _context.Eleves
                .FirstOrDefaultAsync(e => e.IdUtilisateur == idUtilisateur);

            if (eleve == null)
                return new();

            return await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Enseignant)
                        .ThenInclude(ens => ens!.Utilisateur)
                .Where(e =>
                    e.IsPublie &&
                    e.CoursOffert.IdGroupe == eleve.IdGroupe)
                .OrderBy(e => e.JourSemaine)
                .ThenBy(e => e.HeureDebut)
                .Select(e => new
                {
                    id = e.IdEmploi,
                    coursNom = e.CoursOffert.Cours.Nom,
                    enseignantNom =
                        e.CoursOffert.Enseignant.Utilisateur.Prenom + " " +
                        e.CoursOffert.Enseignant.Utilisateur.Nom,
                    jourSemaine = e.JourSemaine.ToString(),
                    heureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin = e.HeureFin.ToString(@"hh\:mm"),
                    salle = e.Salle
                })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetHoraireEnseignantAsync(int idUtilisateur)
        {
            var enseignant = await _context.Enseignants
                .FirstOrDefaultAsync(e => e.IdUtilisateur == idUtilisateur);

            if (enseignant == null)
                return new();

            return await _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Where(e =>
                    e.IsPublie &&
                    e.CoursOffert.IdEnseignant == enseignant.IdEnseignant)
                .OrderBy(e => e.JourSemaine)
                .ThenBy(e => e.HeureDebut)
                .Select(e => new
                {
                    id = e.IdEmploi,
                    coursNom = e.CoursOffert.Cours.Nom,
                    groupeNom = e.CoursOffert.Groupe.Nom,
                    jourSemaine = e.JourSemaine.ToString(),
                    heureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin = e.HeureFin.ToString(@"hh\:mm"),
                    salle = e.Salle
                })
                .ToListAsync<object>();
        }
    }

    public class EmploiDuTempsCreateDto
    {
        public int CoursOffertId { get; set; }

        public int JourSemaine { get; set; }

        public string HeureDebut { get; set; } = "";

        public string HeureFin { get; set; } = "";

        public string? Salle { get; set; }
    }
}
