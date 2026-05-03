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
            return _context.Absences.ToList();
        }

        public Absence? GetAbsenceById(int id)
        {
            return _context.Absences.FirstOrDefault(a => a.IdAbsence == id);
        }

        public List<Absence> GetAbsencesByEleve(int idEleve)
        {
            return _context.Absences
                .Where(a => a.IdEleve == idEleve)
                .ToList();
        }

        public List<Absence> GetAbsencesByUser(int userId)
        {
            var eleve = _context.Eleves.AsNoTracking().FirstOrDefault(e => e.IdUtilisateur == userId);
            if (eleve == null) return new List<Absence>();

            return _context.Absences
                .AsNoTracking()
                .Where(a => a.IdEleve == eleve.IdEleve)
                .ToList();
        }

        public List<Absence> GetAbsencesByCoursOffert(int coursOffertId)
        {
            return _context.Absences
                .Where(a => a.IdCoursOffert == coursOffertId)
                .ToList();
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