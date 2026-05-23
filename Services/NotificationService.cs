using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void CreerNotification(int idUtilisateur, string message)
        {
            _context.Notifications.Add(new Notification
            {
                IdUtilisateur = idUtilisateur,
                Message = message,
                DateCreation = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        public List<Notification> GetNotifications(int idUtilisateur)
        {
            return _context.Notifications
                .Where(n => n.IdUtilisateur == idUtilisateur)
                .OrderByDescending(n => n.DateCreation)
                .ToList();
        }

        public int GetNonLuCount(int idUtilisateur)
        {
            return _context.Notifications
                .Count(n => n.IdUtilisateur == idUtilisateur && !n.IsLue);
        }

        public void MarquerCommeLue(int idNotification)
        {
            var notif = _context.Notifications.Find(idNotification);
            if (notif != null)
            {
                notif.IsLue = true;
                _context.SaveChanges();
            }
        }

        public void MarquerToutesCommeLues(int idUtilisateur)
        {
            var notifs = _context.Notifications
                .Where(n => n.IdUtilisateur == idUtilisateur && !n.IsLue)
                .ToList();
            notifs.ForEach(n => n.IsLue = true);
            _context.SaveChanges();
        }
    }
}
