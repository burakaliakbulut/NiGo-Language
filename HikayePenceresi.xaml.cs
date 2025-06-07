using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using MongoDB.Driver;

namespace NiGoLanguage
{
    public partial class HikayePenceresi : Window
    {
        private readonly HttpClient httpClient = new HttpClient();
        private string kullaniciAdi;
        public HikayePenceresi(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
            txtKullaniciBilgi.Text = $"Hoş geldin, {kullaniciAdi}";
        }

        private async void BtnHikayeOlustur_Click(object sender, RoutedEventArgs e)
        {
            txtIngilizce.Text = "Hikaye oluşturuluyor...";
            txtTurkce.Text = "";

            var kelimeler = await VeritabaniYardimcisi.Kelimeler
                .Find(k => k.Ogrenildi && k.Ingilizce.Length == 5)
                .Limit(10)
                .ToListAsync();

            if (!kelimeler.Any())
            {
                MessageBox.Show("Öğrenilmiş 5 harfli kelime bulunamadı!");
                return;
            }

            string kelimeListesi = string.Join(", ", kelimeler.Select(k => k.Ingilizce));

            string prompt = $"Create a short story using the following English words: {kelimeListesi}. Keep it under 100 words. Then translate it into Turkish.";

            string apiKey = "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"; 
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "openai/gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseText = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseText);
            var fullText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Hikayeyi böl (2 parça varsa)
            var bolunmus = fullText.Split(new[] { "Turkish:", "Türkçe:" }, StringSplitOptions.None);
            txtIngilizce.Text = bolunmus.FirstOrDefault()?.Trim();
            txtTurkce.Text = bolunmus.Skip(1).FirstOrDefault()?.Trim() ?? "(çeviri ayrıştırılamadı)";
        }
    }
}
