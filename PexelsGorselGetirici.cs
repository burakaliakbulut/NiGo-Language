using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Configuration;

public static class PexelsGorselGetirici
{
    private static readonly string apiKey = ConfigurationManager.AppSettings["PexelsKey"];
    private static readonly string klasor = ConfigurationManager.AppSettings["ImageSaveFolder"];

    public static async Task<string> Getir(string kelime)
    {
        try
        {
            Directory.CreateDirectory(klasor);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            var url = $"https://api.pexels.com/v1/search?query={kelime}&per_page=1";
            var jsonStr = await client.GetStringAsync(url);
            var jObj = JObject.Parse(jsonStr);

            var gorselUrl = jObj["photos"]?[0]?["src"]?["medium"]?.ToString();
            if (string.IsNullOrEmpty(gorselUrl))
                return null;

            var dosyaYolu = Path.Combine(klasor, $"{kelime.ToLower()}.jpg");
            var resimVerisi = await client.GetByteArrayAsync(gorselUrl);
            await File.WriteAllBytesAsync(dosyaYolu, resimVerisi);

            return dosyaYolu;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Pexels görsel alma hatası: " + ex.Message);
            return null;
        }
    }
}
