namespace WEBBERBERODEV.Models
{
    public class Randevu
    {
        public int Id { get; set; }
        public string UserId { get; set; } // AspNetUsers tablosundaki kullanıcının Id'si
        public int CalisanId { get; set; }
        public int HizmetId { get; set; }
        public DateTime TarihSaat { get; set; }
        public decimal Fiyat { get; set; }
        public string Durum { get; set; }

        // Navigation properties
        public Calisan Calisan { get; set; }
        public Hizmet Hizmet { get; set; }
        // User bilgisi için direkt AspNetUsers tablosuna gidilir.
        // Burada AspNetUsers ile doğrudan bir navigation yapmak istersen 
        // IdentityUser tipini eklemek ve Identity entegre etmek gerekir.
        // Şimdilik basit kalsın.
    }

}
