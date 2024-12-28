using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBERBERODEV.Models
{
    public class CalisanHizmet
    {

        [Key, Column(Order = 0)]
        public int CalisanId { get; set; }
        public Calisan? Calisan { get; set; }
        [Key, Column(Order = 1)]
        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }
    }
}
