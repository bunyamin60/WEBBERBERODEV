using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WEBBERBERODEV.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace WEBBERBERODEV.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HairRecommendationController : ControllerBase
    {
        private readonly HairstyleChangerService _hairstyleChangerService;

        public HairRecommendationController(HairstyleChangerService hairstyleChangerService)
        {
            _hairstyleChangerService = hairstyleChangerService;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeHairStyle([FromForm] HairRecommendationRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("Lütfen bir resim yükleyin.");
            }

            if (string.IsNullOrEmpty(request.HairstyleStyle))
            {
                return BadRequest("Lütfen bir saç stili seçin.");
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await request.Image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            try
            {
                var processedImageBytes = await _hairstyleChangerService.ChangeHairstyleAsync(imageBytes, request.HairstyleStyle);
                return File(processedImageBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }

    public class HairRecommendationRequest
    {
        public IFormFile Image { get; set; }
        public string HairstyleStyle { get; set; }
    }
}
