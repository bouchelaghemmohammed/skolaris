using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class InscriptionService
    {
        private readonly ApplicationDbContext _context;

        public InscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Inscription> GetAllInscriptions()
        {
            return _context.Inscriptions.ToList();
        }

        public Inscription? GetInscriptionById(int id)
        {
            return _context.Inscriptions
                .FirstOrDefault(i => i.IdInscription == id);
        }

        // Inscriptions d'un élève
        public List<Inscription> GetInscriptionsByUser(int userId)
        {
            return _context.Inscriptions
                .Where(i => i.IdEleve == userId)
                .ToList();
        }

        // Inscriptions à un cours offert
        public List<Inscription> GetInscriptionsByCoursOffert(int coursOffertId)
        {
            return _context.Inscriptions
                .Where(i => i.IdCoursOffert == coursOffertId)
                .ToList();
        }

        // Inscrire un élève à un cours offert
        public bool CreateInscription(Inscription inscription)
        {
            var eleve = _context.Eleves
                .FirstOrDefault(e => e.IdEleve == inscription.IdEleve);

            if (eleve == null)
                return false;

            var coursOffert = _context.CoursOfferts
                .FirstOrDefault(c => c.IdCoursOffert == inscription.IdCoursOffert);

            if (coursOffert == null)
                return false;

            // Empêche les doublons : même élève + même cours offert
            var existe = _context.Inscriptions.Any(i =>
                i.IdEleve == inscription.IdEleve &&
                i.IdCoursOffert == inscription.IdCoursOffert);

            if (existe)
                return false;

            inscription.DateInscription = DateTime.Now;

            _context.Inscriptions.Add(inscription);
            _context.SaveChanges();

            return true;
        }

        public bool UpdateInscription(int id, Inscription updated)
        {
            var inscription = _context.Inscriptions
                .FirstOrDefault(i => i.IdInscription == id);

            if (inscription == null)
                return false;

            inscription.IdEleve = updated.IdEleve;
            inscription.IdCoursOffert = updated.IdCoursOffert;
            inscription.DateInscription = updated.DateInscription;

            _context.SaveChanges();
            return true;
        }

        public bool DeleteInscription(int id)
        {
            var inscription = _context.Inscriptions
                .FirstOrDefault(i => i.IdInscription == id);

            if (inscription == null)
                return false;

            _context.Inscriptions.Remove(inscription);
            _context.SaveChanges();

            return true;
        }
    }
}