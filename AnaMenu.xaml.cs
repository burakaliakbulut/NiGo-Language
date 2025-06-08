using MongoDB.Driver;
using System.Threading.Tasks;
using System.Windows;

namespace NiGoLanguage
{
    public partial class AnaMenu : Window
    {
        private string kullaniciAdi;
        private int kullaniciXP = 0;

        public AnaMenu(string kullanici)
        {
            InitializeComponent();
            kullaniciAdi = kullanici;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ayar = await VeritabaniYardimcisi.Ayarlar
            .Find(x => x.KullaniciAdi == kullaniciAdi)
            .FirstOrDefaultAsync();

            int kullaniciXP = ayar?.XP ?? 0;

            txtXpDurumu.Text = $"XP: {kullaniciXP} | Seviye: {(kullaniciXP / 100) + 1}";

            BtnQuiz.IsEnabled = true;
            BtnSozluk.IsEnabled = true;
            BtnWordle.IsEnabled = kullaniciXP >= 100;
            BtnHikaye.IsEnabled = kullaniciXP >= 200;
            BtnFlashcard.IsEnabled = kullaniciXP >= 500;
        }
        private async Task XPYenile()
        {
            var ayar = await VeritabaniYardimcisi.Ayarlar
                .Find(x => x.KullaniciAdi == kullaniciAdi)
                .FirstOrDefaultAsync();

            int xp = ayar?.XP ?? 0;
            txtXpDurumu.Text = $"XP: {xp} | Seviye: {(xp / 100) + 1}";
        }
        private async void Quiz_Click(object sender, RoutedEventArgs e)
        {
            new Quiz(kullaniciAdi).ShowDialog();
            await XPYenile();
        }

        private void Sozluk_Click(object sender, RoutedEventArgs e)
        {
            new Sozluk().ShowDialog();
        }

        private async void Wordle_Click(object sender, RoutedEventArgs e)
        {
            if (kullaniciXP < 100)
            {
                MessageBox.Show("Bu özelliği açmak için en az 100 XP gerekli.");
                return;
            }

            new WordleOyunu(kullaniciAdi).ShowDialog();
            await XPYenile();
        }

        private void BtnHikaye_Click(object sender, RoutedEventArgs e)
        {
            if (kullaniciXP < 200)
            {
                MessageBox.Show("Bu özelliği açmak için en az 200 XP gerekli.");
                return;
            }

            new HikayePenceresi(kullaniciAdi).ShowDialog();
        }

        private void BtnFlashcard_Click(object sender, RoutedEventArgs e)
        {
            new Flashcard().ShowDialog();
        }
        private void KelimeEkle_Click(object sender, RoutedEventArgs e)
        {
            new KelimeEkle().ShowDialog();
        }

        private void Ayarlar_Click(object sender, RoutedEventArgs e)
        {
            new Ayarlar(kullaniciAdi).ShowDialog();
        }
        private void BtnIstatistik_Click(object sender, RoutedEventArgs e)
        {
            new Istatistik(kullaniciAdi).ShowDialog();
        }
       
        private void BtnCikis_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
    }
}
