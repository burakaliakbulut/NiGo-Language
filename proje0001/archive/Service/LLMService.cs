using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace WordLearningWpfApp.Service
{
    public class LLMService
    {
        private readonly HttpClient _httpClient;
        private const string API_KEY = "your-api-key"; // OpenAI API anahtarınızı buraya ekleyin
        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        public LLMService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
        }

        public async Task<string> GenerateStoryAsync(string[] words)
        {
            var prompt = $"Create a short story using these words: {string.Join(", ", words)}";
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a creative storyteller." },
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsync(
                API_URL,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
                return jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            }

            throw new Exception("Story generation failed.");
        }

        public async Task<string> GenerateImageAsync(string prompt)
        {
            // OpenAI DALL-E API'si için benzer bir implementasyon
            // Bu örnek için sadece bir placeholder
            return "https://example.com/generated-image.jpg";
        }
    }
} 