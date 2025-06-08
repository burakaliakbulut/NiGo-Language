using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;

namespace NiGoLanguage
{
    public partial class Quiz : Window
    {
        private Kelime mevcutKelime;
        private string kullaniciAdi;
        private int gunlukKelimeLimiti = 10;
        private int sorulanKelimeSayisi = 0;
        private IKelimeRepository kelimeRepo = new MongoKelimeRepository();
        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer();


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
            this.Loaded += Quiz_Loaded;
            SoruGetir();
        }
        private void KelimeyiOku(string ingilizceKelime)
        {
            {
                synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.Adult, 0, new System.Globalization.CultureInfo("en-US"));
                synthesizer.SpeakAsync(ingilizceKelime);
            }

        }

        private async void SoruGetir()
        {
            if (sorulanKelimeSayisi >= gunlukKelimeLimiti)
            {
                MessageBox.Show("Bugünlük kelime sınırına ulaştınız!");
                this.Close();
                return;
            }
            var tumKelimeler = await kelimeRepo.OgrenilmemisleriGetirAsync();
            var uygunKelimeler = new List<Kelime>();

            foreach (var kelime in tumKelimeler)
            {
                var cozum = await VeritabaniYardimcisi.KelimeCozumler
                    .Find(x => x.KelimeId == kelime.Id && x.KullaniciAdi == kullaniciAdi)
                    .FirstOrDefaultAsync();

                int tekrarIndex = cozum?.DogruCozumTarihleri?.Count ?? 0;
                if (tekrarIndex >= 6)
                    continue;

                if (cozum == null || tekrarIndex == 0)
                {
                    uygunKelimeler.Add(kelime); // hiç çözülmemiş kelime
                }
                else
                {
                    var sonCozumTarihi = cozum.DogruCozumTarihleri.Last();
                    var tekrarZamani = tekrarAraliklari[tekrarIndex];
                    if (DateTime.Now - sonCozumTarihi >= tekrarZamani)
                    {
                        uygunKelimeler.Add(kelime); // zamanı gelmiş tekrar
                    }
                }
            }

            if (!uygunKelimeler.Any())
            {
                MessageBox.Show("Bugünlük tekrar edecek kelime yok.");
                this.Close();
                return;
            }

            var rnd = new Random();
            Kelime yeniKelime;
            do
            {
                yeniKelime = uygunKelimeler[rnd.Next(uygunKelimeler.Count)];
            } while (uygunKelimeler.Count > 1 && yeniKelime.Id == mevcutKelime?.Id);

            mevcutKelime = yeniKelime;
            txtSoru.Text = $"Kelime telaffuz ediliyor...\nTürkçe karşılığını yazın:";
            KelimeyiOku(mevcutKelime.Ingilizce);
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
            bool dogruMu = cevap == mevcutKelime.Turkce.ToLower(); // İngilizce kelime veriliyor, Türkçesi isteniyor.

            var filtre = Builders<KelimeCozum>.Filter.Eq(x => x.KelimeId, mevcutKelime.Id) &
                         Builders<KelimeCozum>.Filter.Eq(x => x.KullaniciAdi, kullaniciAdi);
            var cozum = await VeritabaniYardimcisi.KelimeCozumler.Find(filtre).FirstOrDefaultAsync();

            if (dogruMu)
            {
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

                    await XpEkle(50);
                    MessageBox.Show("Tebrikler! Bu kelime artık öğrenildi. +50 XP kazandınız!");
                }
                else
                {
                    await XpEkle(10);
                    MessageBox.Show($"Doğru! ({tekrarSayisi}/6 tekrar yapıldı)");
                }
            }
            else
            {
                if (cozum == null)
                {
                    cozum = new KelimeCozum
                    {
                        KelimeId = mevcutKelime.Id,
                        KullaniciAdi = kullaniciAdi,
                        YanlisCozumTarihleri = new List<DateTime> { DateTime.Now }
                    };
                    await VeritabaniYardimcisi.KelimeCozumler.InsertOneAsync(cozum);
                }
                else
                {
                    cozum.YanlisCozumTarihleri.Add(DateTime.Now);
                    await VeritabaniYardimcisi.KelimeCozumler.ReplaceOneAsync(filtre, cozum);
                }

                MessageBox.Show($"Yanlış cevap. Doğru cevap: {mevcutKelime.Turkce}");
            }

            txtCevap.Clear();
            sorulanKelimeSayisi++;
            SoruGetir();
        }


    }
}
