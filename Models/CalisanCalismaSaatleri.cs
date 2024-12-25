using System;
using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class CalisanCalismaSaatleri
    {
        public int Id { get; set; }

        [Required]
        public int CalisanId { get; set; }
        public Calisan Calisan { get; set; }

        [Required]
        public DayOfWeek Gun { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BaslangicSaati { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BitisSaati { get; set; }
    }
}
