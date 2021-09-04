using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordFinder5000.Core
{
    public interface IBookService
    {
        Task<List<string>> GetTopWordsAsync();
    }

    public class BookService : IBookService
    {
        private readonly IBookRepo _bookRepo;
        private readonly IFilerParser _filerParser;
        private readonly AppSettings _appSettings;

        public BookService(IBookRepo bookRepo, IFilerParser filerParser, AppSettings appSettings)
        {
            _bookRepo = bookRepo;
            _filerParser = filerParser;
            _appSettings = appSettings;
        }

        public async Task<List<string>> GetTopWordsAsync()
        {
            ValidateSettings();

            var content = await _bookRepo.GetBookAsync(_appSettings.SourceUrl);

            var parsed = _filerParser.Parse(content);

            var wordCounts = GetWordCounts(parsed);

            return OrderAndExactWords(wordCounts);
        }

        private void ValidateSettings()
        {
            if (_appSettings.TopWordCount < 1)
                throw new ArgumentException("TopWordCount setting must be greater than zero",
                    nameof(_appSettings.TopWordCount));

            if (!Uri.IsWellFormedUriString(_appSettings.SourceUrl, UriKind.Absolute))
                throw new ArgumentException("Source URL is invalid", nameof(_appSettings.SourceUrl));
        }

        private List<string> OrderAndExactWords(Dictionary<string, int> wordCounts)
        {
            return wordCounts.OrderByDescending(key => key.Value).Select(keyVal => keyVal.Key)
                .Take(_appSettings.TopWordCount).ToList();
        }

        private Dictionary<string, int> GetWordCounts(List<string> content)
        {
            var wordCounts = new Dictionary<string, int>();
            
            foreach (var word in content)
            {
                var lower = word.ToLower();

                lower = RemoveSuffix(lower);

                if (_appSettings.Excluded.Contains(lower))
                    continue;

                if (wordCounts.ContainsKey(lower))
                {
                    wordCounts[lower]++;
                }
                else
                    wordCounts.Add(lower, 1);
            }

            return wordCounts;
        }

        private string RemoveSuffix(string word)
        {
            var index = word.IndexOfAny(SpecialCharacters.Apostrophes.ToArray());
            return index != -1 ? word.Substring(0, index) : word;
        }
    }
}