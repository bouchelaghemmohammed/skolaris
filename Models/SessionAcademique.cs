namespace Skolaris.Models
{
    public class SessionAcademique
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;  // Automne, Hiver, Été

        public int Annee { get; set; }

        public DateTime DateDebut { get; set; }

        public DateTime DateFin { get; set; }

        public int EtablissementId { get; set; }

        public bool IsActive { get; set; } = true;

        public Etablissement Etablissement { get; set; } = null!;

        public ICollection<Cours> Cours { get; set; } = new List<Cours>();
    }
}
