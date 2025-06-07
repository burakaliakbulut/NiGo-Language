using MongoDB.Driver;
using System.Windows;

namespace NiGoLanguage
{
    public partial class Ayarlar : Window
    {
        private string kullaniciAdi;
        
        public Ayarlar(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
            AyariYukle();
        }

        private async void AyariYukle()
        {
            var ayar = await VeritabaniYardimcisi.Ayarlar
                .Find(x => x.KullaniciAdi == kullaniciAdi)
                .FirstOrDefaultAsync();

            if (ayar != null)
            {
                txtKelimeSayisi.Text = ayar.GunlukKelimeSayisi.ToString();
            }
            else
            {
                txtKelimeSayisi.Text = "10"; // varsayılan değer
            }
        }

        private async void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtKelimeSayisi.Text.Trim(), out int sayi) || sayi <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir sayı giriniz.");
                return;
            }

            var filtre = Builders<KullaniciAyar>.Filter.Eq(x => x.KullaniciAdi, kullaniciAdi);
            var guncelle = Builders<KullaniciAyar>.Update.Set(x => x.GunlukKelimeSayisi, sayi);

            var sonuc = await VeritabaniYardimcisi.Ayarlar.UpdateOneAsync(filtre, guncelle, new UpdateOptions { IsUpsert = true });

            MessageBox.Show("Ayar başarıyla kaydedildi.");
        }
    }
}
