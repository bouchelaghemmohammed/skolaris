namespace Skolaris.Models
{
    public class ConversationParticipant
    {
        public int IdConversation { get; set; }
        public int IdUtilisateur { get; set; }
        public DateTime DateAdhesion { get; set; } = DateTime.UtcNow;

        public Conversation Conversation { get; set; } = null!;
        public Utilisateur Utilisateur { get; set; } = null!;
    }
}
