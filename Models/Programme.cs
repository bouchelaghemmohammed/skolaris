namespace Skolaris.Models
{
    public class Programme
    {
        public int IdProgramme { get; set; }
        public string Nom { get; set; } = string.Empty;

        public ICollection<Niveau> Niveaux { get; set; } = new List<Niveau>();
        public ICollection<Cours> Cours { get; set; } = new List<Cours>();
        public ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();
        public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
    }
}
