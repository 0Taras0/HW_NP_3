using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services
{
    public class ServerService
    {
        private const string BaseUrl = "http://localhost:5111";

        public async Task<(bool IsSuccess, string ImageUrl, string ErrorMessage)> UploadImageAsync(string imagePath)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                string apiUrl = $"{BaseUrl}/api/galleries/upload";

                var uploadModel = new { Photo = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(imagePath))}" };

                StringContent content = new StringContent(JsonSerializer.Serialize(uploadModel), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    var imageUrl = JsonDocument.Parse(responseString).RootElement.GetProperty("image").GetString();

                    if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                    {
                        imageUrl = new Uri(new Uri(BaseUrl), imageUrl).ToString();
                    }

                    return (true, imageUrl, null);
                }

                return (false, null, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, string ImageUrl, string ErrorMessage)> SearchImageAsync(string imageUrl)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl);

                return response.IsSuccessStatusCode ? (true, imageUrl, null) : (false, null, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
