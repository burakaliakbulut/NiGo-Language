using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NiGoLanguage
{
    public class WordleOyunuManager
    {
        public string HedefKelime { get; private set; }
        public int KalanHak { get; private set; } = 6;
        public bool OyunBitti => KalanHak == 0 || KazandiMi;
        public bool KazandiMi { get; private set; } = false;

        public async Task OyunuBaslatAsync()
        {
            HedefKelime = await RasgeleBesHarfliKelimeGetir();
            KalanHak = 6;
            KazandiMi = false;
        }

        private async Task<string> RasgeleBesHarfliKelimeGetir()
        {
            var filtre = Builders<Kelime>.Filter.Where(k => k.Ingilizce.Length == 5 && k.Ogrenildi);
            var kelimeler = await VeritabaniYardimcisi.Kelimeler.Find(filtre).ToListAsync();

            if (kelimeler.Count == 0)
                return "APPLE"; // yedek

            var rnd = new Random();
            return kelimeler[rnd.Next(kelimeler.Count)].Ingilizce.ToUpper();
        }

        public string TahminYap(string tahmin)
        {
            if (OyunBitti)
                return "✖✖✖✖✖"; // dummy dönüş

            tahmin = tahmin.ToUpper().Trim();
            if (tahmin.Length != 5)
                return "✖✖✖✖✖";

            KalanHak--;

            if (tahmin == HedefKelime)
            {
                KazandiMi = true;
                return "✔✔✔✔✔";
            }

            return Degerlendir(tahmin);
        }

        private string Degerlendir(string giris)
        {
            char[] sonuc = new char[5];

            for (int i = 0; i < 5; i++)
            {
                if (giris[i] == HedefKelime[i])
                    sonuc[i] = '✔'; // doğru yerde
                else if (HedefKelime.Contains(giris[i]))
                    sonuc[i] = '?'; // kelimede var ama yanlış yerde
                else
                    sonuc[i] = '✖'; // hiç yok
            }

            return new string(sonuc);
        }
    }
}
