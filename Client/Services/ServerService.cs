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
        // Базова URL-адреса для підключення до сервера.
        private const string BaseUrl = "http://localhost:5111";

        // Метод для завантаження зображення на сервер.
        // Приймає шлях до зображення, повертає результат виконання (успішність, URL зображення, повідомлення про помилку).
        public async Task<(bool IsSuccess, string ImageUrl, string ErrorMessage)> UploadImageAsync(string imagePath)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                string apiUrl = $"{BaseUrl}/api/galleries/upload"; // URL API для завантаження зображень.

                // Формування даних для завантаження у форматі JSON.
                var uploadModel = new { Photo = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(imagePath))}" };

                // Формування HTTP-запиту з JSON-контентом.
                StringContent content = new StringContent(JsonSerializer.Serialize(uploadModel), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content); // Відправка POST-запиту.

                // Обробка відповіді від сервера.
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    var imageUrl = JsonDocument.Parse(responseString).RootElement.GetProperty("image").GetString();

                    // Перевірка коректності URL-адреси зображення.
                    if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                    {
                        imageUrl = new Uri(new Uri(BaseUrl), imageUrl).ToString();
                    }

                    return (true, imageUrl, null); // Успішне виконання.
                }

                return (false, null, await response.Content.ReadAsStringAsync()); // Помилка з боку сервера.
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message); // Помилка під час виконання запиту.
            }
        }

        // Метод для пошуку зображення за URL-адресою.
        // Приймає URL зображення, повертає результат виконання (успішність, URL зображення, повідомлення про помилку).
        public async Task<(bool IsSuccess, string ImageUrl, string ErrorMessage)> SearchImageAsync(string imageUrl)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl); // Відправка GET-запиту.

                // Перевірка статусу відповіді.
                return response.IsSuccessStatusCode ? (true, imageUrl, null) : (false, null, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message); // Помилка під час виконання запиту.
            }
        }
    }
}
