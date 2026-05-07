using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class AbsenceService
    {
        private readonly ApplicationDbContext _context;

        public AbsenceService(ApplicationDbContext context)
        {
            _context = context;
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
                    Statut = Enums.StatutJustification.Justifiee,
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

        public bool CreateAbsence(Absence absence)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdEleve == absence.IdEleve);

            if (eleve == null)
                return false;

            _context.Absences.Add(absence);
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