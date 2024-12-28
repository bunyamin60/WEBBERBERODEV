using System.Collections.Generic;

namespace WEBBERBERODEV.Models
{
    public class HomePageViewModel
    {
        public string SalonAdi { get; set; }
        public string AcilisKapanisSaati { get; set; }
        public List<Hizmet> Hizmetler { get; set; }
        public List<Calisan> Calisanlar { get; set; }
    }
}
