using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Interfaces;

public interface IWordService
{
    Task<Word> AddWordAsync(Word word);
    Task<Word> UpdateWordAsync(Word word);
    Task<bool> DeleteWordAsync(string wordId);
    Task<Word?> GetWordAsync(string wordId);
    Task<List<Word>> GetWordsAsync(int skip = 0, int limit = 20);
    Task<List<Word>> GetDailyWordsAsync(string userId, int count = 10);
    Task<List<Word>> SearchWordsAsync(string searchTerm);
    Task<bool> UpdateWordProgressAsync(string wordId, string userId, bool isCorrect);
    Task<List<Word>> GetWordsByDifficultyAsync(int difficulty, int limit = 20);
    Task<List<Word>> GetWordsByCategoryAsync(string category, int limit = 20);
} 