namespace Skolaris.Models
{
    public class Etablissement
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string Adresse { get; set; } = string.Empty;

        public string? Telephone { get; set; }

        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<SessionAcademique> Sessions { get; set; } = new List<SessionAcademique>();
    }
}
