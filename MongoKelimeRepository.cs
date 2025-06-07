using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiGoLanguage
{
    public class MongoKelimeRepository : IKelimeRepository
    {
        private readonly IMongoCollection<Kelime> _koleksiyon;

        public MongoKelimeRepository()
        {
            _koleksiyon = VeritabaniYardimcisi.Kelimeler;
        }

        public async Task<List<Kelime>> TumunuGetirAsync()
        {
            return await _koleksiyon.Find(_ => true).ToListAsync();
        }

        public async Task<List<Kelime>> OgrenilmemisleriGetirAsync()
        {
            return await _koleksiyon.Find(k => !k.Ogrenildi).ToListAsync();
        }

        public async Task<Kelime> IdIleGetirAsync(string id)
        {
            return await _koleksiyon.Find(k => k.Id == id).FirstOrDefaultAsync();
        }

        public async Task EkleAsync(Kelime kelime)
        {
            await _koleksiyon.InsertOneAsync(kelime);
        }

        public async Task GuncelleAsync(Kelime kelime)
        {
            var filtre = Builders<Kelime>.Filter.Eq(k => k.Id, kelime.Id);
            await _koleksiyon.ReplaceOneAsync(filtre, kelime);
        }

        public async Task SilAsync(string id)
        {
            await _koleksiyon.DeleteOneAsync(k => k.Id == id);
        }
    }

}
