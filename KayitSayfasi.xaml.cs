using MongoDB.Driver;
using System.Windows;

namespace NiGoLanguage;

public partial class KayitSayfasi : Window
{
    public KayitSayfasi()
    {
        InitializeComponent();
    }

    private async void BtnKayitOl_Click(object sender, RoutedEventArgs e)
    {
        var kullaniciAdi = txtKullaniciAdi.Text.Trim();
        var sifre = txtSifre.Password;
        
        if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
        {
            MessageBox.Show("Kullanıcı adı ve şifre boş olamaz.");
            return;
        }

        var filtre = Builders<Kullanici>.Filter.Eq(k => k.KullaniciAdi, kullaniciAdi);
        var varMi = await VeritabaniYardimcisi.Kullanicilar.Find(filtre).FirstOrDefaultAsync();

        if (varMi != null)
        {
            MessageBox.Show("Bu kullanıcı adı zaten kayıtlı.");
            return;
        }

        var yeniKullanici = new Kullanici
        {
            KullaniciAdi = kullaniciAdi,
            Sifre = sifre
        };
        await VeritabaniYardimcisi.Kullanicilar.InsertOneAsync(yeniKullanici);
        var yeniAyar = new KullaniciAyar
        {
            KullaniciAdi = yeniKullanici.KullaniciAdi,
            GunlukKelimeSayisi = 10,
            XP = 0
        };
        await VeritabaniYardimcisi.Ayarlar.InsertOneAsync(yeniAyar);

        MessageBox.Show("Kayıt başarılı! Giriş yapabilirsiniz.");
        this.Close(); // Giriş ekranına dön
    }
}
