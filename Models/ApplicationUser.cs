using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WEBBERBERODEV.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Ad {  get; set; }
        
        [Required]
        public string Soyad { get; set; }
        
    }
}
