using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class RapportAbsenceDto
    {
        public int IdAbsence { get; set; }
        public DateTime DateAbsence { get; set; }
        public string Type { get; set; } = "";
        public string Statut { get; set; } = "";
        public string EleveNom { get; set; } = "";
        public string EleveMatricule { get; set; } = "";
        public string CoursNom { get; set; } = "";
        public string? JustificationStatut { get; set; }
        public string? JustificationDescription { get; set; }
    }

    public class AbsenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public AbsenceService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public List<Absence> GetAllAbsences()
        {
            return _context.Absences.Include(a => a.Justification).ToList();
        }

        public Absence? GetAbsenceById(int id)
        {
            return _context.Absences.Include(a => a.Justification).FirstOrDefault(a => a.IdAbsence == id);
        }

        public List<Absence> GetAbsencesByEleve(int idEleve)
        {
            return _context.Absences
                .Include(a => a.Justification)
                .Where(a => a.IdEleve == idEleve)
                .ToList();
        }

        public List<Absence> GetAbsencesByUser(int userId)
        {
            var eleve = _context.Eleves.AsNoTracking().FirstOrDefault(e => e.IdUtilisateur == userId);
            if (eleve == null) return new List<Absence>();

            return _context.Absences
                .AsNoTracking()
                .Include(a => a.Justification)
                .Where(a => a.IdEleve == eleve.IdEleve)
                .ToList();
        }

        public List<Absence> GetAbsencesByCoursOffert(int coursOffertId)
        {
            return _context.Absences
                .Include(a => a.Justification)
                .Where(a => a.IdCoursOffert == coursOffertId)
                .ToList();
        }

        public bool SetExplicationEleve(int idAbsence, string texte)
        {
            var absence = _context.Absences.Include(a => a.Justification).FirstOrDefault(a => a.IdAbsence == idAbsence);
            if (absence == null) return false;

            if (absence.Justification == null)
            {
                absence.Justification = new JustificationAbsence
                {
                    IdAbsence = idAbsence,
                    Statut = Enums.StatutJustification.NonJustifiee,
                    Description = texte
                };
                _context.JustificationsAbsence.Add(absence.Justification);
            }
            else
            {
                absence.Justification.Description = texte;
            }

            _context.SaveChanges();
            return true;
        }

        public bool SetJustification(int idAbsence, Enums.StatutJustification statut, string? description)
        {
            var absence = _context.Absences.Include(a => a.Justification).FirstOrDefault(a => a.IdAbsence == idAbsence);
            if (absence == null) return false;

            if (absence.Justification == null)
            {
                absence.Justification = new JustificationAbsence
                {
                    IdAbsence = idAbsence,
                    Statut = statut,
                    Description = description
                };
                _context.JustificationsAbsence.Add(absence.Justification);
            }
            else
            {
                absence.Justification.Statut = statut;
                absence.Justification.Description = description;
            }

            _context.SaveChanges();
            return true;
        }

        public List<RapportAbsenceDto> GetRapport(int? idEleve, int? idGroupe, int? idCoursOffert)
        {
            var absences = _context.Absences
                .Include(a => a.Justification)
                .Include(a => a.Eleve).ThenInclude(e => e!.Utilisateur)
                .Include(a => a.CoursOffert).ThenInclude(co => co!.Cours)
                .AsQueryable();

            if (idEleve.HasValue)
                absences = absences.Where(a => a.IdEleve == idEleve.Value);

            if (idCoursOffert.HasValue)
                absences = absences.Where(a => a.IdCoursOffert == idCoursOffert.Value);

            if (idGroupe.HasValue)
                absences = absences.Where(a => a.Eleve!.IdGroupe == idGroupe.Value);

            return absences
                .OrderByDescending(a => a.DateAbsence)
                .Select(a => new RapportAbsenceDto
                {
                    IdAbsence = a.IdAbsence,
                    DateAbsence = a.DateAbsence,
                    Type = a.Type.ToString(),
                    Statut = a.Statut.ToString(),
                    EleveNom = a.Eleve != null ? $"{a.Eleve.Utilisateur!.Prenom} {a.Eleve.Utilisateur.Nom}" : $"#{a.IdEleve}",
                    EleveMatricule = a.Eleve != null ? a.Eleve.Matricule : "",
                    CoursNom = a.CoursOffert != null && a.CoursOffert.Cours != null ? a.CoursOffert.Cours.Nom : $"Cours #{a.IdCoursOffert}",
                    JustificationStatut = a.Justification != null ? a.Justification.Statut.ToString() : null,
                    JustificationDescription = a.Justification != null ? a.Justification.Description : null
                })
                .ToList();
        }

        public bool CreateAbsence(Absence absence)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdEleve == absence.IdEleve);
            if (eleve == null) return false;

            _context.Absences.Add(absence);
            _context.SaveChanges();

            var coursOffert = _context.CoursOfferts
                .Include(co => co.Cours)
                .FirstOrDefault(co => co.IdCoursOffert == absence.IdCoursOffert);
            var coursNom = coursOffert?.Cours?.Nom ?? $"Cours #{absence.IdCoursOffert}";

            _notificationService.CreerNotification(
                eleve.IdUtilisateur,
                $"Une absence a été enregistrée le {absence.DateAbsence:yyyy-MM-dd} pour le cours {coursNom}."
            );

            var eleveUser = _context.Utilisateurs.FirstOrDefault(u => u.IdUtilisateur == eleve.IdUtilisateur);
            var eleveNom = eleveUser != null ? $"{eleveUser.Prenom} {eleveUser.Nom}" : $"Élève #{eleve.IdEleve}";

            var totalAbsences = _context.Absences.Count(a => a.IdEleve == eleve.IdEleve);
            if (totalAbsences == 3)
            {
                var admins = _context.Utilisateurs
                    .Where(u => u.Role == Enums.Role.ADMIN)
                    .ToList();
                foreach (var admin in admins)
                {
                    _notificationService.CreerNotification(
                        admin.IdUtilisateur,
                        $"⚠️ Seuil atteint : {eleveNom} a accumulé 3 absences. Un suivi est recommandé."
                    );
                }
            }

            return true;
        }

        public bool ChangerStatut(int id, Enums.StatutAbsence statut)
        {
            var absence = _context.Absences.FirstOrDefault(a => a.IdAbsence == id);
            if (absence == null) return false;

            absence.Statut = statut;
            _context.SaveChanges();
            return true;
        }

        public bool UpdateAbsence(int id, Absence updated)
        {
            var absence = _context.Absences.FirstOrDefault(a => a.IdAbsence == id);

            if (absence == null)
                return false;

            absence.IdEleve = updated.IdEleve;
            absence.IdCoursOffert = updated.IdCoursOffert;
            absence.DateAbsence = updated.DateAbsence;
            absence.Type = updated.Type;
            absence.Statut = updated.Statut;

            _context.SaveChanges();
            return true;
        }

        public bool DeleteAbsence(int id)
        {
            var absence = _context.Absences.FirstOrDefault(a => a.IdAbsence == id);

            if (absence == null)
                return false;

            _context.Absences.Remove(absence);
            _context.SaveChanges();
            return true;
        }
    }
}