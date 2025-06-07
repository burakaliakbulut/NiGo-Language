using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiGoLanguage
{
    public interface IKelimeRepository
    {
        Task<List<Kelime>> TumunuGetirAsync();
        Task<List<Kelime>> OgrenilmemisleriGetirAsync();
        Task<Kelime> IdIleGetirAsync(string id);
        Task EkleAsync(Kelime kelime);
        Task GuncelleAsync(Kelime kelime);
        Task SilAsync(string id);
    }
}
