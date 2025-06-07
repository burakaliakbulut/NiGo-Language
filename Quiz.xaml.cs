using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Speech.Synthesis;

namespace NiGoLanguage
{
    public partial class Quiz : Window
    {
        private Kelime mevcutKelime;
        private string kullaniciAdi;
        private int gunlukKelimeLimiti = 10;
        private int sorulanKelimeSayisi = 0;
        private IKelimeRepository kelimeRepo = new MongoKelimeRepository();

        private static readonly TimeSpan[] tekrarAraliklari = new[]
        {
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(7),
            TimeSpan.FromDays(30),
            TimeSpan.FromDays(90),
            TimeSpan.FromDays(180),
            TimeSpan.FromDays(365)
        };

        public Quiz(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
            SoruGetir();
        }
        private async void Quiz_Loaded(object sender, RoutedEventArgs e)
        {
            await AyariYukle();
            SoruGetir();
        }
        private void KelimeyiOku(string ingilizceKelime)
        {
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.Adult, 0, new System.Globalization.CultureInfo("en-US"));
            synthesizer.SpeakAsync(ingilizceKelime);
        }

        private async void SoruGetir()
        {
            var kelimeler = await kelimeRepo.OgrenilmemisleriGetirAsync();
            if (sorulanKelimeSayisi >= gunlukKelimeLimiti)
            {
                MessageBox.Show("Bugünkü günlük tekrar hedefinize ulaştınız!");
                this.Close();
                return;
            }
            if (!kelimeler.Any())
            {
                MessageBox.Show("Çözülecek kelime kalmadı!");
                this.Close();
                return;
            }

            var rnd = new Random();
            mevcutKelime = kelimeler[rnd.Next(kelimeler.Count)];
            txtSoru.Text = $"Kelime telaffuz ediliyor... \nTürkçe karşılığını yazın:";
            KelimeyiOku(mevcutKelime.Ingilizce); // 🔈 Sesli okuma

        }
        private async Task XpEkle(int miktar)
        {
            var filtre = Builders<KullaniciAyar>.Filter.Eq(x => x.KullaniciAdi, kullaniciAdi);
            var guncelle = Builders<KullaniciAyar>.Update.Inc(x => x.XP, miktar);
            await VeritabaniYardimcisi.Ayarlar.UpdateOneAsync(filtre, guncelle);
        }


        private async Task AyariYukle()
        {
            var ayar = await VeritabaniYardimcisi.Ayarlar
                .Find(x => x.KullaniciAdi == kullaniciAdi)
                .FirstOrDefaultAsync();

            gunlukKelimeLimiti = ayar?.GunlukKelimeSayisi ?? 10;
        }

        private async void BtnCevapla_Click(object sender, RoutedEventArgs e)
        {
            var cevap = txtCevap.Text.Trim().ToLower();
            if (cevap == mevcutKelime.Turkce.ToLower())
            {
                var filtre = Builders<KelimeCozum>.Filter.Eq(x => x.KelimeId, mevcutKelime.Id) &
                             Builders<KelimeCozum>.Filter.Eq(x => x.KullaniciAdi, kullaniciAdi);

                var cozum = await VeritabaniYardimcisi.KelimeCozumler.Find(filtre).FirstOrDefaultAsync();

                if (cozum == null)
                {
                    cozum = new KelimeCozum
                    {
                        KelimeId = mevcutKelime.Id,
                        KullaniciAdi = kullaniciAdi,
                        DogruCozumTarihleri = new List<DateTime> { DateTime.Now }
                    };
                    await VeritabaniYardimcisi.KelimeCozumler.InsertOneAsync(cozum);
                }
                else
                {
                    cozum.DogruCozumTarihleri.Add(DateTime.Now);
                    await VeritabaniYardimcisi.KelimeCozumler.ReplaceOneAsync(filtre, cozum);
                }

                int tekrarSayisi = cozum.DogruCozumTarihleri.Count;

                if (tekrarSayisi >= 6)
                {
                    var kelimeFiltre = Builders<Kelime>.Filter.Eq(k => k.Id, mevcutKelime.Id);
                    var guncelle = Builders<Kelime>.Update.Set(k => k.Ogrenildi, true);
                    await VeritabaniYardimcisi.Kelimeler.UpdateOneAsync(kelimeFiltre, guncelle);

                    await XpEkle(50); // 🎯 5 kelime öğrenince +50 XP
                    MessageBox.Show("Tebrikler! Bu kelime artık öğrenildi. +50 XP kazandınız!");
                }
                else
                {
                    await XpEkle(10); // her doğru cevapta +10 XP
                    MessageBox.Show($"Doğru! ({tekrarSayisi}/6 tekrar yapıldı)");
                    
                }

                txtCevap.Clear();
                SoruGetir();
            }
            else
            {
                MessageBox.Show($"Yanlış cevap. Doğru cevap: {mevcutKelime.Ingilizce}");
                txtCevap.Clear();
                SoruGetir();
            }
        }
    }
}
