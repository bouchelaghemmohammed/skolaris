using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Bulletin
    {
        public int IdBulletin { get; set; }
        public decimal MoyenneGeneral { get; set; }
        public Mention Mention { get; set; }
        public int IdEleve { get; set; }
        public int IdSession { get; set; }

        public Eleve Eleve { get; set; } = null!;
        public Session Session { get; set; } = null!;
        public ICollection<DetailBulletin> DetailBulletins { get; set; } = new List<DetailBulletin>();
    }
}
