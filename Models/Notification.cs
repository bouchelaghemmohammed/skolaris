using System.ComponentModel.DataAnnotations;

namespace Skolaris.Models
{
    public class Notification
    {
        [Key]
        public int IdNotification { get; set; }
        public int IdUtilisateur { get; set; }
        public string Message { get; set; } = "";
        public bool IsLue { get; set; } = false;
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public Utilisateur? Utilisateur { get; set; }
    }
}
