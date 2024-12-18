namespace WEBBERBERODEV.Models
{
    public class CalisanHizmet
    {
        public int CalisanId { get; set; }
        public int HizmetId { get; set; }

        public Calisan Calisan { get; set; }
        public Hizmet Hizmet { get; set; }
    
    }
}
