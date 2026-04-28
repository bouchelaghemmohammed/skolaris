using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.ViewModels;

namespace Skolaris.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public DashboardStatsViewModel GetAdminStats()
        {
            return new DashboardStatsViewModel
            {
                TotalUsers = _context.Utilisateurs.Count(),
                TotalAdmins = _context.Utilisateurs.Count(u => u.Role == Role.ADMIN),
                TotalEnseignants = _context.Utilisateurs.Count(u => u.Role == Role.ENSEIGNANT),
                TotalEleves = _context.Utilisateurs.Count(u => u.Role == Role.ELEVE),
                ActiveUsers = _context.Utilisateurs.Count(u => u.IsActive),
                InactiveUsers = _context.Utilisateurs.Count(u => !u.IsActive)
            };
        }
    }
}
