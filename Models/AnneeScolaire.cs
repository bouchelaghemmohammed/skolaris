namespace Skolaris.Models
{
    public class AnneeScolaire
    {
        public int IdAnnee { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public int IdEcole { get; set; }

        public Ecole Ecole { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
