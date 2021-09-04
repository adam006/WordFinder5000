using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using WordFinder5000.Core;
using Xunit;

namespace WordFinder5000.Tests
{
    public class BookServiceTests
    {
        private readonly Faker _faker;
        private readonly IBookService _bookService;
        private readonly Mock<IBookRepo> _bookRepo;
        private readonly string _bookContent;
        private readonly int _topWordCount;
        private readonly string _url;
        private readonly Mock<IFilerParser> _filerParser;
        private readonly List<string> _bookWordsList;
        private readonly AppSettings _appSettings;

        public BookServiceTests()
        {
            _faker = new Faker();

            _url = _faker.Internet.Url();

            _topWordCount = _faker.Random.Int(1, 50);
            _bookContent = _faker.Random.Words(1000).ToLower();

            _bookWordsList = _bookContent.Split(' ').Distinct().ToList();

            _bookRepo = new Mock<IBookRepo>();
            _bookRepo.Setup(s => s.GetBookAsync(_url)).ReturnsAsync(_bookContent);

            _appSettings = new AppSettings
            {
                SourceUrl = _url,
                TopWordCount = _topWordCount,
                Excluded = new HashSetIgnoreCase()
            };

            _filerParser = new Mock<IFilerParser>();
            _filerParser.Setup(s => s.Parse(_bookContent)).Returns(_bookWordsList);

            _bookService = new BookService(_bookRepo.Object, _filerParser.Object, _appSettings);
        }

        [Fact]
        public async Task GetTopWords_RepoException_ShouldThrowException()
        {
            _bookRepo.Setup(s => s.GetBookAsync(_url)).ThrowsAsync(new Exception());

            Func<Task> func = async () => await _bookService.GetTopWordsAsync();

            await func.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetTopWords_ValidData_ShouldReturnExpectedTopCount()
        {
            var result = await _bookService.GetTopWordsAsync();

            result.Count.Should().Be(_topWordCount);
        }

        [Fact]
        public async Task GetTopWords_ValidData_WordsResultShouldContainMostUsedWordsDescending()
        {
            var words = _faker.Random.WordsArray(1000).Distinct().Select(s => s.ToLower()).ToList();

            var topWords = words.Take(_topWordCount).ToList();

            for (int i = 0; i < _topWordCount; i++)
            {
                words.AddRange(Enumerable.Repeat(topWords[i], _topWordCount - i));
            }

            _filerParser.Setup(s => s.Parse(_bookContent)).Returns(words);

            var result = await _bookService.GetTopWordsAsync();

            result.Should().ContainInOrder(topWords);
        }

        [Fact]
        public async Task GetTopWords_TopWordCountGreaterThanWordsFound_ShouldReturnWordsFound()
        {
            _appSettings.TopWordCount = _bookWordsList.Count + _faker.Random.Int(1, 100);

            var result = await _bookService.GetTopWordsAsync();

            result.Count.Should().Be(_bookWordsList.Count);
        }

        [Fact]
        public async Task GetTopWords_ValidData_ShouldNotHaveExcludedWords()
        {
            var excluded = _bookWordsList.Distinct().Take(_faker.Random.Int(1, 100));
            _appSettings.Excluded = new HashSetIgnoreCase(excluded);

            var result = await _bookService.GetTopWordsAsync();

            result.Any(s => _appSettings.Excluded.Any(a => a.Equals(s, StringComparison.OrdinalIgnoreCase))).Should()
                .BeFalse();
        }

        [Fact]
        public async Task GetTopWords_MultiCase_ShouldNotHaveExcludedWords()
        {
            var excluded = _bookWordsList.Distinct().Take(_faker.Random.Int(1, _bookWordsList.Count / 2))
                .Select(s => s.ToUpper());

            var words = new List<string>(_bookWordsList.Select(s => s.ToLower()));
            _filerParser.Setup(s => s.Parse(_bookContent)).Returns(words);

            _appSettings.Excluded = new HashSetIgnoreCase(excluded);
            ;

            var result = await _bookService.GetTopWordsAsync();

            result.Any(s => _appSettings.Excluded.Any(a => a.Equals(s, StringComparison.OrdinalIgnoreCase))).Should()
                .BeFalse();
        }

        [Fact]
        public async Task GetTopWords_MultiCaseWords_ShouldCountAsSameWord()
        {
            var words = new List<string>();
            var expectedWord = _faker.Random.Word().ToLower();

            words.Add(expectedWord);
            words.Add(expectedWord.ToUpper());
            words.Add(char.ToUpper(expectedWord[0]) + expectedWord[1..]);

            var content = string.Join(' ', words);

            _bookRepo.Setup(s => s.GetBookAsync(_url)).ReturnsAsync(content);
            _filerParser.Setup(s => s.Parse(content)).Returns(words);

            var result = await _bookService.GetTopWordsAsync();

            result.Should().BeEquivalentTo(expectedWord);
        }

        [Fact]
        public async Task GetTopWords_TopWordCountLessThan1_ShouldThrowArgumentException()
        {
            _appSettings.TopWordCount = _faker.Random.Int(int.MinValue, 0);

            Func<Task> func = async () => await _bookService.GetTopWordsAsync();

            await func.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetTopWords_InvalidUrl_ShouldThrowArgumentException()
        {
            _appSettings.SourceUrl = _faker.Random.String2(10);

            Func<Task> func = async () => await _bookService.GetTopWordsAsync();

            await func.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetTopWords_ShouldRemoveSuffixWhenApostrophe_ShouldCountAsSameWord()
        {
            var wordList = new List<string> { "ship", "ship's", "ship’s", "ship're", "Ahab", "Ahab’s", "Ahab'nt", "something", "else" };

            var content = string.Join(' ', wordList);

            _appSettings.TopWordCount = 2;
            
            _bookRepo.Setup(s => s.GetBookAsync(_url)).ReturnsAsync(content);
            _filerParser.Setup(s => s.Parse(content)).Returns(wordList);

            var result = await _bookService.GetTopWordsAsync();

            result.Should().ContainInOrder(new List<string> { "ship", "ahab" });
        }
    }
}