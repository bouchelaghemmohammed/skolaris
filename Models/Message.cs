namespace Skolaris.Models
{
    public class Message
    {
        public int IdMessage { get; set; }
        public string? Contenu { get; set; }
        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
        public int IdConversation { get; set; }
        public int IdExpediteur { get; set; }

        public Conversation Conversation { get; set; } = null!;
        public Utilisateur Expediteur { get; set; } = null!;
        public ICollection<MessageUtilisateur> MessageUtilisateurs { get; set; } = new List<MessageUtilisateur>();
    }
}
