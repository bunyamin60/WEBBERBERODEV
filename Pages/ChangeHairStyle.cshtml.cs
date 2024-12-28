using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEBBERBERODEV.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace WEBBERBERODEV.Pages
{
    public class ChangeHairStyleModel : PageModel
    {
        private readonly HairstyleChangerService _hairstyleChangerService;

        public ChangeHairStyleModel(HairstyleChangerService hairstyleChangerService)
        {
            _hairstyleChangerService = hairstyleChangerService;
        }

        [BindProperty]
        public IFormFile Image { get; set; }

        [BindProperty]
        public string HairstyleStyle { get; set; }

        public string ProcessedImage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Image == null || string.IsNullOrEmpty(HairstyleStyle))
            {
                ModelState.AddModelError("", "Lütfen tüm alanlarý doldurun.");
                return Page();
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await Image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            try
            {
                var processedImageBytes = await _hairstyleChangerService.ChangeHairstyleAsync(imageBytes, HairstyleStyle);
                ProcessedImage = $"data:image/jpeg;base64,{Convert.ToBase64String(processedImageBytes)}";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bir hata oluþtu: {ex.Message}");
            }

            return Page();
        }
    }
}
