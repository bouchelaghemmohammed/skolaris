using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Note
    {
        public int IdNote { get; set; }
        public decimal Valeur { get; set; }
        public TypeNote Type { get; set; }
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }

        public Eleve Eleve { get; set; } = null!;
        public CoursOffert CoursOffert { get; set; } = null!;
        public ICollection<DetailBulletin> DetailBulletins { get; set; } = new List<DetailBulletin>();
    }
}
