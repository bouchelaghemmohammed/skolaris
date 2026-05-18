namespace Skolaris.Models
{
    public class DetailBulletin
    {
        public int IdDetailBulletin { get; set; }
        public int IdBulletin { get; set; }
        public int IdNote { get; set; }
        public int IdCoursOffert { get; set; }

        public Bulletin Bulletin { get; set; } = null!;
        public Note Note { get; set; } = null!;
        public CoursOffert CoursOffert { get; set; } = null!;
    }
}
