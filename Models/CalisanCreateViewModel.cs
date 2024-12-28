using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class CalisanCreateViewModel
    {
        // Identity tarafında lazım olan bilgiler
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Calisan tablosu için
        [Required]
        public string Ad { get; set; }

        [Required]
        public string Soyad { get; set; }

        [Required]
        public string Uzmanlik { get; set; }

        public bool AktifMi { get; set; }


    }
}
