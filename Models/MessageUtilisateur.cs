namespace Skolaris.Models
{
    public class MessageUtilisateur
    {
        public int IdMessageUtilisateur { get; set; }
        public int IdMessage { get; set; }
        public int IdDestinataire { get; set; }
        public bool EstLu { get; set; } = false;
        public DateTime? DateLecture { get; set; }

        public Message Message { get; set; } = null!;
    }
}
