using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Conversation
    {
        public int IdConversation { get; set; }
        public string? Sujet { get; set; }
        public TypeConversation Type { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
