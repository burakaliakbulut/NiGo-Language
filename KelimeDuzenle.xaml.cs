using MongoDB.Driver;
using System.Windows;

namespace NiGoLanguage
{
    public partial class KelimeDuzenle : Window
    {
        private Kelime kelime;

        public KelimeDuzenle(Kelime secilen)
        {
            InitializeComponent();
            kelime = secilen;

            txtIngilizce.Text = kelime.Ingilizce;
            txtTurkce.Text = kelime.Turkce;
            txtOrnek.Text = kelime.OrnekCumle;
        }

        private async void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            kelime.Ingilizce = txtIngilizce.Text.Trim();
            kelime.Turkce = txtTurkce.Text.Trim();
            kelime.OrnekCumle = txtOrnek.Text.Trim();

            var filtre = Builders<Kelime>.Filter.Eq(k => k.Id, kelime.Id);
            await VeritabaniYardimcisi.Kelimeler.ReplaceOneAsync(filtre, kelime);

            MessageBox.Show("Kelime güncellendi.");
            this.Close();
        }
    }
}
