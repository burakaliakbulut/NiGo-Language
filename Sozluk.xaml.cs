using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NiGoLanguage
{
    public partial class Sozluk : Window
    {
        private List<Kelime> kelimeListesi;
        private IKelimeRepository kelimeRepo = new MongoKelimeRepository();


        public Sozluk()
        {
            InitializeComponent();
            Yukle();
        }

        private async void Yukle()
        {
            kelimeListesi = await VeritabaniYardimcisi.Kelimeler.Find(_ => true).ToListAsync();
            dgKelimeler.ItemsSource = kelimeListesi;
        }

        private void BtnYenile_Click(object sender, RoutedEventArgs e)
        {
            Yukle();
        }

        private async void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgKelimeler.SelectedItem is Kelime secilen)
            {
                var sonuc = MessageBox.Show($"'{secilen.Ingilizce}' kelimesini silmek istiyor musunuz?", "Silme Onayı", MessageBoxButton.YesNo);
                if (sonuc == MessageBoxResult.Yes)
                {
                    await kelimeRepo.SilAsync(secilen.Id);
                    MessageBox.Show("Kelime silindi.");
                    Yukle();
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir kelime seçiniz.");
            }
        }

        private void BtnDuzenle_Click(object sender, RoutedEventArgs e)
        {
            if (dgKelimeler.SelectedItem is Kelime secilen)
            {
                var duzenlePencere = new KelimeDuzenle(secilen);
                duzenlePencere.ShowDialog();
                Yukle(); // Güncel veriyi yeniden yükle
            }
            else
            {
                MessageBox.Show("Lütfen bir kelime seçiniz.");
            }
        }
    }
}
