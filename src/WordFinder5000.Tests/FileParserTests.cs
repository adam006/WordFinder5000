using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bogus;
using FluentAssertions;
using WordFinder5000.Core;
using Xunit;

namespace WordFinder5000.Tests
{
    public class FileParserTests
    {
        private readonly Faker _faker;
        private readonly IFilerParser _filerParser;
        private readonly string _content;
        private List<string> _wordList;

        public FileParserTests()
        {
            _faker = new Faker();

            _filerParser = new FileParser();

            _wordList = _faker.Make(1000, () => _faker.Random.String2(2, 10)).ToList();
            _content = string.Join(' ', _wordList);
        }

        [Fact]
        public void Parse_ContentLongerThanZero_ShouldReturnListOfContent()
        {
            var result = _filerParser.Parse(_content);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_NoContent_ShouldThrowArgumentException()
        {
            Action act = () => _filerParser.Parse("");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Parse_ContentContainsApostrophe_ShouldNotRemoveApostrophe()
        {
            var expected = new List<string> { "Ahab’s", "Boat", "and", "Crew", "Fedallah", "Whale’s" };
            var content = string.Join(' ', expected);

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Parse_ContentContainsApostrophe_ShouldNotRemoveSingleQuote()
        {
            var expected = new List<string> { "don't", "stop", "believe'n" };
            var content = string.Join(' ', expected);

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Parse_ContentContainsLineBreaks_ShouldRemoveLineBreaks()
        {
            var contentWithLineBreaks = _content.Replace(" ", " \r\n");

            var result = _filerParser.Parse(contentWithLineBreaks);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_ContentContainsExtraSpaces_ShouldRemoveExtraSpaces()
        {
            var contentWithExtraSpaces = _content.Replace(" ", "  ");

            var result = _filerParser.Parse(contentWithExtraSpaces);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_ContentContainsExcludedCharactersAtEnd_ShouldRemoveSpecialChars()
        {
            var contentWithSpecialCharacters = _content + " " + string.Join(' ', SpecialCharacters.NotAllowed);
            var result = _filerParser.Parse(contentWithSpecialCharacters);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_SpecialCharactersBetweenWords_ShouldCountEachWord()
        {
            var specialCount = SpecialCharacters.NotAllowed.Count;
            var sb = new StringBuilder();

            var words = new List<string>();
            for (int i = 0; i < specialCount; i++)
            {
                var word = _faker.Random.String2(5);
                words.Add(word);
                sb.Append(word + SpecialCharacters.NotAllowed[i]);
            }

            var content = sb.ToString();

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(words);
        }

        [Fact]
        public void Parse_SpecialCharacterBeforeWord_ShouldCountWord()
        {
            var content = string.Join(' ',
                _wordList.Select(s => s = SpecialCharacters.NotAllowed[_faker.Random.Int(0, SpecialCharacters.NotAllowed.Count - 1)] + s));

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_ContainsNumbers_ShouldReturnOnlyWords()
        {
            var numbers = _faker.Make(10, () => _faker.Random.Int(1000, 9999));
            var content = _content + " " + string.Join(' ', numbers);

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_EndsWithNumber_ShouldIgnoreNumber()
        {
            var content = string.Join(' ', _wordList.Select(s => s + _faker.Random.Int()));

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(_wordList);
        }

        [Fact]
        public void Parse_StartsWithNumber_ShouldIgnoreNumber()
        {
            var content = string.Join(' ', _wordList.Select(s => _faker.Random.Int() + s));

            var result = _filerParser.Parse(content);

            result.Should().BeEquivalentTo(_wordList);
        }
    }
}