using Microsoft.Win32;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Measure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NiGoLanguage
{
    public partial class Istatistik : Window
    {
        private string kullaniciAdi;
        private string raporMetni = "";

        public Istatistik(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
            IstatislikleriYukle();
        }

        private async void IstatislikleriYukle()
        {
            var toplamKelime = await VeritabaniYardimcisi.Kelimeler.CountDocumentsAsync(_ => true);
            var ogrenilenKelime = await VeritabaniYardimcisi.Kelimeler.CountDocumentsAsync(k => k.Ogrenildi);
            var oran = toplamKelime == 0 ? 0 : (double)ogrenilenKelime / toplamKelime * 100;

            var cozumler = await VeritabaniYardimcisi.KelimeCozumler
                .Find(c => c.KullaniciAdi == kullaniciAdi)
                .ToListAsync();

            var toplamCozum = cozumler.Sum(c => c.DogruCozumTarihleri.Count);
            var ortalamaTekrar = cozumler.Any() ? toplamCozum / cozumler.Count : 0;

            raporMetni = $"""
                Toplam kelime sayısı       : {toplamKelime}
                Öğrenilen kelime sayısı    : {ogrenilenKelime}
                Öğrenme oranı              : %{oran:F2}

                Toplam doğru tekrar        : {toplamCozum}
                Ortalama tekrar/kelime     : {ortalamaTekrar}

                Bu veriler 6 tekrar prensibine göre değerlendirilmiştir.
                NiGo Language ile öğrenmeye devam edin! 🚀
                """;

            txtOzet.Text = raporMetni;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IstatislikleriYukle();
        }
        private async void Yukselt()
        {
            // Ozet metni
            var ogrenilen = await VeritabaniYardimcisi.Kelimeler
                .CountDocumentsAsync(k => k.Ogrenildi);
            txtOzet.Text = $"Toplam öğrenilen kelime: {ogrenilen}";

            // Grafik için örnek veri: XP artışları (örnek)
            var cozumler = await VeritabaniYardimcisi.KelimeCozumler
                .Find(x => x.KullaniciAdi == kullaniciAdi)
                .ToListAsync();

            var tarihGrup = cozumler
                .SelectMany(c => c.DogruCozumTarihleri)
                .GroupBy(t => t.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Tarih = g.Key.ToShortDateString(), Adet = g.Count() })
                .ToList();

            xpChart.Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = tarihGrup.Select(x => x.Adet).ToList(),
                    Name = "Doğru Cevap"
                }
            };

            xpChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = tarihGrup.Select(x => x.Tarih).ToList()
                }
            };
        }

        private void BtnKapat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void xpChart_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
