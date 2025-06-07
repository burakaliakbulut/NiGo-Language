using Microsoft.Win32;
using MongoDB.Driver;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;

namespace NiGoLanguage
{
    public partial class KelimeEkle : Window
    {
        public KelimeEkle()
        {
            InitializeComponent();
        }

        private void BtnResimSec_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dosyaSec = new OpenFileDialog();
            dosyaSec.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";

            if (dosyaSec.ShowDialog() == true)
            {
                txtResimYolu.Text = dosyaSec.FileName;
            }
        }


        private async void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var kelime = new Kelime
            {
                Ingilizce = txtIngilizce.Text.Trim(),
                Turkce = txtTurkce.Text.Trim(),
                OrnekCumle = txtOrnekCumle.Text.Trim()
            };

            if (string.IsNullOrWhiteSpace(kelime.OrnekCumle) || string.IsNullOrWhiteSpace(kelime.Turkce))
            {
                var (enCumle, trCeviri) = await OrnekCumleOlusturucu.CumleVeCeviriGetir(kelime.Ingilizce);

                if (string.IsNullOrWhiteSpace(kelime.OrnekCumle))
                    kelime.OrnekCumle = enCumle ?? "No sentence generated.";

                if (string.IsNullOrWhiteSpace(kelime.Turkce))
                    kelime.Turkce = trCeviri ?? "Çeviri bulunamadı.";
            }
            if (string.IsNullOrWhiteSpace(kelime.OrnekCumle))
            {
                kelime.OrnekCumle = await OrnekCumleOlusturucu.OrnekCumleGetir(kelime.Ingilizce)
                                     ?? "No example generated.";
            }
            // Kullanıcı resim seçmemişse AI ile getir
            if (string.IsNullOrWhiteSpace(txtResimYolu.Text))
            {
                string otomatikYol = await PexelsGorselGetirici.Getir(kelime.Ingilizce);
                kelime.ResimYolu = otomatikYol ?? "";
            }
            else
            {
                kelime.ResimYolu = txtResimYolu.Text;
            }

            await VeritabaniYardimcisi.Kelimeler.InsertOneAsync(kelime);
            MessageBox.Show("Kelime başarıyla kaydedildi.");

            // ✅ Görseli ekranda göster
            if (!string.IsNullOrWhiteSpace(kelime.ResimYolu) && File.Exists(kelime.ResimYolu))
            {
                imgOnizleme.Source = new BitmapImage(new Uri(kelime.ResimYolu));
            }
        }


    }
}
    