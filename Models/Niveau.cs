namespace Skolaris.Models
{
    public class Niveau
    {
        public int IdNiveau { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int IdProgramme { get; set; }

        public Programme Programme { get; set; } = null!;
        public ICollection<Cours> Cours { get; set; } = new List<Cours>();
        public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
    }
}
