using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Utilisateur
    {
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        // Navigation
        public Enseignant? Enseignant { get; set; }
        public Eleve? Eleve { get; set; }
        public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> MessagesEnvoyes { get; set; } = new List<Message>();
    }
}
