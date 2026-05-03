using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.ViewModels;

namespace Skolaris.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<UserListViewModel> GetAllUsers()
        {
            return _context.Utilisateurs
                .Select(u => new UserListViewModel
                {
                    Id = u.IdUtilisateur,
                    Nom = u.Prenom + " " + u.Nom,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive
                })
                .ToList();
        }

        public bool ToggleActive(int id, int currentUserId)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.IdUtilisateur == id);
            if (user == null) return false;
            if (user.IdUtilisateur == currentUserId) return false;

            user.IsActive = !user.IsActive;
            _context.SaveChanges();
            return true;
        }

        public bool ChangeRole(int id, string role)
        {
            if (!Enum.TryParse<Role>(role, true, out var roleEnum))
                return false;

            var user = _context.Utilisateurs.FirstOrDefault(u => u.IdUtilisateur == id);
            if (user == null) return false;

            user.Role = roleEnum;
            _context.SaveChanges();
            return true;
        }

        // ADM-11 Mise a jour profil
        public bool UpdateProfile(int id, string prenom, string nom, string email)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
                return false;

            if (!email.Contains("@"))
                return false;

            var user = _context.Utilisateurs.FirstOrDefault(u => u.IdUtilisateur == id);
            if (user == null)
                return false;

            user.Prenom = string.IsNullOrWhiteSpace(prenom) ? user.Prenom : prenom;
            user.Nom = nom;
            user.Email = email;

            _context.SaveChanges();
            return true;
        }
    }
}
