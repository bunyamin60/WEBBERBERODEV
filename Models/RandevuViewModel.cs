using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class RandevuViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Çalışan")]
        public int CalisanId { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi ve Saati")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Randevu tarihi gelecekte olmalıdır.")]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        [Display(Name = "Seçilen Hizmetler")]
        public List<int> HizmetIds { get; set; } = new List<int>();
    }

    // Gelecek tarihler için doğrulama attribute'u
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now;
            }
            return false;
        }
    }
}
