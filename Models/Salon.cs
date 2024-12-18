using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class Salon
    {
        public int Id { get; set; }
        [Required]
        public string Ad { get; set; }
        public TimeSpan AcilisSaati { get; set; }
        public TimeSpan KapanisSaati { get; set; }

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<Calisan> Calisanlar { get; set; } = new List<Calisan>();
    }
}
