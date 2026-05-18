namespace Skolaris.Models
{
    public class Ecole
    {
        public int IdEcole { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<AnneeScolaire> AnneesScolaires { get; set; } = new List<AnneeScolaire>();
    }
}
