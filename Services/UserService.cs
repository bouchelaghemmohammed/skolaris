using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;
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
                    IsActive = u.IsActive,
                    Telephone = u.Telephone
                })
                .ToList();
        }

        public bool UpdateTelephone(int id, string? telephone)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.IdUtilisateur == id);
            if (user == null) return false;
            user.Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone.Trim();
            _context.SaveChanges();
            return true;
        }

        public List<UserListViewModel> GetByGroupe(int idGroupe)
        {
            return _context.Eleves
                .Where(e => e.IdGroupe == idGroupe)
                .Select(e => new UserListViewModel
                {
                    Id = e.Utilisateur.IdUtilisateur,
                    Nom = e.Utilisateur.Prenom + " " + e.Utilisateur.Nom,
                    Email = e.Utilisateur.Email,
                    Role = "ELEVE",
                    IsActive = e.Utilisateur.IsActive,
                    Telephone = e.Utilisateur.Telephone
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

            // Vérifier que l'email n'est pas déjà utilisé par un autre utilisateur
            var emailPrisParAutre = _context.Utilisateurs
                .Any(u => u.Email.ToLower() == email.Trim().ToLower() && u.IdUtilisateur != id);
            if (emailPrisParAutre)
                return false;

            user.Prenom = string.IsNullOrWhiteSpace(prenom) ? user.Prenom : prenom;
            user.Nom = nom;
            user.Email = email.Trim();

            _context.SaveChanges();
            return true;
        }

        public (bool Success, string Error) CreateUser(string prenom, string nom, string email, string role, string motDePasse, string? telephone = null)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
                return (false, "Champs obligatoires manquants.");

            if (!email.Contains("@"))
                return (false, "Email invalide.");

            if (!Enum.TryParse<Role>(role, true, out var roleEnum))
                return (false, "Rôle invalide.");

            if (_context.Utilisateurs.Any(u => u.Email.ToLower() == email.Trim().ToLower()))
                return (false, "Cet email est déjà utilisé.");

            var user = new Utilisateur
            {
                Prenom = prenom.Trim(),
                Nom = nom.Trim(),
                Email = email.Trim(),
                Role = roleEnum,
                IsActive = true,
                Telephone = string.IsNullOrWhiteSpace(telephone) ? null : telephone.Trim()
            };

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Utilisateur>();
            user.MotDePasse = hasher.HashPassword(user, motDePasse);

            _context.Utilisateurs.Add(user);
            _context.SaveChanges();
            return (true, "");
        }
    }
}
