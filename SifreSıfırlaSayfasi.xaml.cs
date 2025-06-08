using MongoDB.Driver;
using System.Windows;

namespace NiGoLanguage;

public partial class SifreSifirlaSayfasi : Window
{
    public SifreSifirlaSayfasi()
    {
        InitializeComponent();
    }

    private async void BtnSifreGuncelle_Click(object sender, RoutedEventArgs e)
    {
        var kullaniciAdi = txtKullaniciAdi.Text.Trim();
        var yeniSifre = txtYeniSifre.Password;

        if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(yeniSifre))
        {
            MessageBox.Show("Lütfen tüm alanları doldurun.");
            return;
        }

        var filtre = Builders<Kullanici>.Filter.Eq(k => k.KullaniciAdi, kullaniciAdi);
        var guncelleme = Builders<Kullanici>.Update.Set(k => k.Sifre, yeniSifre);

        var sonuc = await VeritabaniYardimcisi.Kullanicilar.UpdateOneAsync(filtre, guncelleme);

        if (sonuc.ModifiedCount > 0)
        {
            MessageBox.Show("Şifre başarıyla güncellendi.");
            this.Close();
        }
        else
        {
            MessageBox.Show("Kullanıcı bulunamadı!");
        }
    }
}
