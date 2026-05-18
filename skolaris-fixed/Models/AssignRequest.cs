using System.ComponentModel.DataAnnotations;

namespace Skolaris.Models
{
    public class AssignRequest
    {
        [Range(1, int.MaxValue)]
        public int IdEnseignant { get; set; }

        [Range(1, int.MaxValue)]
        public int IdGroupe { get; set; }
    }
}
