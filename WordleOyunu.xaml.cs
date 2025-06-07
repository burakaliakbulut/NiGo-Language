using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NiGoLanguage
{
    public partial class WordleOyunu : Window
    {
        private WordleOyunuManager oyun;
        private string kullaniciAdi;

        public WordleOyunu(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
            txtKullaniciBilgi.Text = $"Hoş geldin, {kullaniciAdi}";
            oyun = new WordleOyunuManager();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await oyun.OyunuBaslatAsync();

                if (string.IsNullOrWhiteSpace(oyun.HedefKelime))
                {
                    MessageBox.Show("Kelime yüklenemedi. Lütfen internet bağlantınızı veya veritabanını kontrol edin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTahmin.IsEnabled = false;
                    return;
                }

                txtDurum.Text = $"Tahmin hakkı: {oyun.KalanHak}";
                tahminAlani.Children.Clear();
                txtTahmin.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oyun başlatılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task XpEkle(int miktar)
        {
            var filtre = Builders<KullaniciAyar>.Filter.Eq(x => x.KullaniciAdi, kullaniciAdi);
            var guncelle = Builders<KullaniciAyar>.Update.Inc(x => x.XP, miktar);
            await VeritabaniYardimcisi.Ayarlar.UpdateOneAsync(filtre, guncelle);
        }

        private async void BtnTahmin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(oyun.HedefKelime))
            {
                MessageBox.Show("Oyun başlatılamadı. Lütfen yeniden başlatmayı deneyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string giris = txtTahmin.Text.Trim().ToUpper();
            if (giris.Length != 5)
            {
                MessageBox.Show("Lütfen 5 harfli bir kelime girin.");
                return;
            }

            string sonuc = oyun.TahminYap(giris);
            txtTahmin.Clear();

            StackPanel satir = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            for (int i = 0; i < 5; i++)
            {
                Border kutu = new Border
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(GetRenk(sonuc[i])),
                    Child = new TextBlock
                    {
                        Text = giris[i].ToString(),
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    }
                };
                satir.Children.Add(kutu);
            }

            tahminAlani.Children.Add(satir);

            if (oyun.OyunBitti)
            {
                if (oyun.KazandiMi)
                {
                    txtDurum.Text = "🎉 Tebrikler! Bildiniz. +15 XP";
                    await XpEkle(15); // ✅ XP EKLENİYOR
                }
                else
                {
                    txtDurum.Text = $"❌ Oyun bitti. Doğru kelime: {oyun.HedefKelime}";
                }

                txtTahmin.IsEnabled = false;
            }
            else
            {
                txtDurum.Text = $"Tahmin hakkı: {oyun.KalanHak}";
            }
        }

        private void BtnYenidenBaslat_Click(object sender, RoutedEventArgs e)
        {
            Window_Loaded(sender, e);
        }

        private Color GetRenk(char durum)
        {
            return durum switch
            {
                '✔' => Colors.Green,
                '?' => Colors.Goldenrod,
                '✖' => Colors.DimGray,
                _ => Colors.Black
            };
        }
    }
}