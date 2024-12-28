using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace WEBBERBERODEV.Services
{
    public class HairstyleChangerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HairstyleChangerService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RapidAPI:HairstyleChangerKey"];
        }

        public async Task<byte[]> ChangeHairstyleAsync(byte[] imageBytes, string hairstyleStyle)
        {
            // API'ye gönderilecek veriyi form-data olarak ayarla
            var formData = new MultipartFormDataContent
            {
                { new ByteArrayContent(imageBytes), "image_target", "uploaded_image.jpg" },
                { new StringContent(hairstyleStyle), "hair_type" }
            };

            // HTTP POST isteği hazırla
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://hairstyle-changer.p.rapidapi.com/huoshan/facebody/hairstyle"), // Doğru endpoint'i kullanın
                Content = formData
            };

            // Gerekli başlıkları ekle
            request.Headers.Add("x-rapidapi-host", "hairstyle-changer.p.rapidapi.com");
            request.Headers.Add("x-rapidapi-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                // Hata yönetimi
                throw new Exception($"API isteği başarısız oldu: {response.StatusCode}");
            }

            // API yanıtını byte dizisi olarak döndür
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
