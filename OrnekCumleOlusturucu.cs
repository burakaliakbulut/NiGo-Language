using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class OrnekCumleOlusturucu
{
    private static readonly string apiKey = "AIzaSyBdqnRBioBs0abhz3gAHNO_6Wm_OIqYHa8";
    private static readonly string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=" + apiKey;
    public static async Task<(string ingilizce, string turkce)> CumleVeCeviriGetir(string kelime)
    {
        var prompt = $"Use the word '{kelime}' in a simple English sentence, and then translate it into Turkish. Format: Sentence (English) + newline + Translation (Turkish).";

        var requestObj = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var json = JsonSerializer.Serialize(requestObj);
        using var client = new HttpClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode)
            return (null, null);

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        try
        {
            var metin = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var satirlar = metin.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
            return satirlar.Length >= 2
                ? (satirlar[0].Trim(), satirlar[1].Trim())
                : (satirlar[0].Trim(), null);
        }
        catch
        {
            return (null, null);
        }
    }

    public static async Task<string> OrnekCumleGetir(string kelime)
    {
        var prompt = $"Create a meaningful example sentence in English using the word '{kelime}'.";

        var requestObj = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestObj);
        using var client = new HttpClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        try
        {
            var sentence = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return sentence;
        }
        catch
        {
            return null;
        }
    }
}
