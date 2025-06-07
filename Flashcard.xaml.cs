using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NiGoLanguage
{
    public partial class Flashcard : Window
    {
        private List<Kelime> kartlar;
        private int indeks = 0;
        private bool cevapGosterildi = false;

        public Flashcard()
        {
            InitializeComponent();
            KartlariYukle();
        }

        private async void KartlariYukle()
        {
            // Öğrenilmiş kelimelerden getir
            kartlar = await VeritabaniYardimcisi.Kelimeler
                .Find(k => k.Ogrenildi)
                .Limit(100)
                .ToListAsync();

            if (kartlar.Count == 0)
            {
                MessageBox.Show("Henüz öğrenilmiş kelimeniz yok.");
                this.Close();
                return;
            }

            indeks = 0;
            cevapGosterildi = false;
            txtKart.Text = kartlar[indeks].Ingilizce;
        }

        private void BtnCevabiGoster_Click(object sender, RoutedEventArgs e)
        {
            if (!cevapGosterildi)
            {
                txtKart.Text = kartlar[indeks].Turkce;
                cevapGosterildi = true;
            }
            else
            {
                txtKart.Text = kartlar[indeks].Ingilizce;
                cevapGosterildi = false;
            }
        }

        private void BtnBildim_Click(object sender, RoutedEventArgs e)
        {
            SonrakiKartaGec();
        }

        private void BtnGec_Click(object sender, RoutedEventArgs e)
        {
            SonrakiKartaGec();
        }

        private void SonrakiKartaGec()
        {
            indeks++;
            if (indeks >= kartlar.Count)
            {
                MessageBox.Show("Tüm flashcard'ları tamamladınız!");
                this.Close();
                return;
            }

            cevapGosterildi = false;
            txtKart.Text = kartlar[indeks].Ingilizce;
        }
    }
}
