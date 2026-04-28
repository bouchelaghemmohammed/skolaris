using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Session
    {
        public int IdSession { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public TypeSession Type { get; set; }
        public int IdAnnee { get; set; }
        public bool IsActive { get; set; } = true;

        public AnneeScolaire AnneeScolaire { get; set; } = null!;
        public ICollection<CoursOffert> CoursOfferts { get; set; } = new List<CoursOffert>();
        public ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
        public ICollection<PaiementEleve> PaiementsEleves { get; set; } = new List<PaiementEleve>();
        public ICollection<PaiementEnseignant> PaiementsEnseignants { get; set; } = new List<PaiementEnseignant>();
    }
}
