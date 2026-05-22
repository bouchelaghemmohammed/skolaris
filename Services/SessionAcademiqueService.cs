using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class SessionAcademiqueService
    {
        private readonly ApplicationDbContext _context;

        public SessionAcademiqueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<object> GetAll()
        {
            return _context.Sessions
                .Include(s => s.AnneeScolaire)
                    .ThenInclude(a => a.Ecole)
                .Select(s => (object)new
                {
                    s.IdSession,
                    s.Libelle,
                    Type = s.Type.ToString(),
                    s.IsActive,
                    s.IdAnnee,
                    AnneeLibelle = s.AnneeScolaire.Libelle,
                    EtablissementId = s.AnneeScolaire.IdEcole,
                    EtablissementNom = s.AnneeScolaire.Ecole.Nom
                })
                .ToList();
        }

        public List<Session> GetByAnnee(int idAnnee) =>
            _context.Sessions.Where(s => s.IdAnnee == idAnnee).ToList();

        public Session? GetById(int id) =>
            _context.Sessions.Include(s => s.AnneeScolaire).FirstOrDefault(s => s.IdSession == id);

        public Session Create(string libelle, TypeSession type, int idAnnee)
        {
            var session = new Session
            {
                Libelle = libelle,
                Type = type,
                IdAnnee = idAnnee,
                IsActive = true
            };
            _context.Sessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        public bool Update(int id, string libelle, TypeSession type, bool isActive)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == id);
            if (session == null) return false;

            session.Libelle = libelle;
            session.Type = type;
            session.IsActive = isActive;
            _context.SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == id);
            if (session == null) return false;

            _context.Sessions.Remove(session);
            _context.SaveChanges();
            return true;
        }
    }
}
