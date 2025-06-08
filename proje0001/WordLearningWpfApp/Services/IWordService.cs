using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IWordService
    {
        Task<Word> GetWordByIdAsync(string wordId);
        Task<IEnumerable<Word>> GetWordsByUserIdAsync(string userId);
        Task<IEnumerable<Word>> GetDailyWordsAsync(string userId);
        Task<Word> CreateWordAsync(Word word);
        Task<Word> UpdateWordAsync(Word word);
        Task<bool> DeleteWordAsync(string wordId);
        Task<bool> MarkWordAsLearnedAsync(string wordId, string userId);
        Task<bool> MarkWordAsDifficultAsync(string wordId, string userId);
        Task<IEnumerable<Word>> SearchWordsAsync(string userId, string searchTerm);
        Task<IEnumerable<Word>> GetLearnedWordsAsync(string userId);
        Task<IEnumerable<Word>> GetDifficultWordsAsync(string userId);
        Task<IEnumerable<Word>> GetAllWordsAsync(string userId);
        Task<IEnumerable<string>> GetCategoriesAsync(string userId);
        Task<IEnumerable<string>> GetTagsAsync(string userId);
        Task<IEnumerable<Word>> GetDictionaryWordsAsync(string userId);
        Task<bool> AddWordToLearningAsync(string userId, string wordId);
        Task<IEnumerable<Word>> SearchDictionaryWordsAsync(string userId, string searchTerm);
        Task<bool> AddWordAsync(string userId, Word word);
        Task<bool> UpdateWordProgressAsync(string userId, string wordId, WordProgress progress);
    }
} 