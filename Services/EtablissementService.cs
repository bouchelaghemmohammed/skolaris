using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class EtablissementService
    {
        private readonly ApplicationDbContext _context;

        public EtablissementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Ecole> GetAll() => _context.Ecoles.ToList();

        public Ecole? GetById(int id) =>
            _context.Ecoles.FirstOrDefault(e => e.IdEcole == id);

        public Ecole Create(string nom, string? adresse, string? telephone, string? email)
        {
            var ecole = new Ecole
            {
                Nom = nom,
                Adresse = adresse,
                Telephone = telephone,
                Email = email,
                IsActive = true
            };
            _context.Ecoles.Add(ecole);
            _context.SaveChanges();
            return ecole;
        }

        public bool Update(int id, string nom, string? adresse, string? telephone, string? email)
        {
            var ecole = _context.Ecoles.FirstOrDefault(x => x.IdEcole == id);
            if (ecole == null) return false;

            ecole.Nom = nom;
            ecole.Adresse = adresse;
            ecole.Telephone = telephone;
            ecole.Email = email;
            _context.SaveChanges();
            return true;
        }

        public bool ToggleActive(int id)
        {
            var ecole = _context.Ecoles.FirstOrDefault(x => x.IdEcole == id);
            if (ecole == null) return false;

            ecole.IsActive = !ecole.IsActive;
            _context.SaveChanges();
            return true;
        }
    }
}
