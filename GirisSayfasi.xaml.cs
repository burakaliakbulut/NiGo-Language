using MongoDB.Driver;
using System.Windows;
using System.Windows.Input;

namespace NiGoLanguage;

public partial class GirisSayfasi : Window
{
    public GirisSayfasi()
    {
        InitializeComponent();
    }
    private void BtnKayitOl_Click(object sender, RoutedEventArgs e)
    {
        var kayitEkrani = new KayitSayfasi();
        kayitEkrani.ShowDialog();
    }

    private async void BtnGirisYap_Click(object sender, RoutedEventArgs e)
    {
        var kullaniciAdi = txtKullaniciAdi.Text.Trim();
        var sifre = txtSifre.Password;

        var filtre = Builders<Kullanici>.Filter.Eq(k => k.KullaniciAdi, kullaniciAdi) &
                     Builders<Kullanici>.Filter.Eq(k => k.Sifre, sifre);

        var sonuc = await VeritabaniYardimcisi.Kullanicilar.Find(filtre).FirstOrDefaultAsync();

        if (sonuc != null)
        {
            MessageBox.Show("Giriş başarılı!");
            var anaMenu = new AnaMenu(kullaniciAdi);
            anaMenu.Show();
            this.Close(); // Giriş penceresini kapat

        }
        else
        {
            MessageBox.Show("Kullanıcı adı veya şifre yanlış!");
        }

    }
    private void SifreUnuttum_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show("Lütfen kayıtlı olduğunuz e-posta adresiyle destek birimine başvurun.\n\nBu özellik henüz uygulamada aktif değil.",
                        "Şifremi Unuttum",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
    }

}
