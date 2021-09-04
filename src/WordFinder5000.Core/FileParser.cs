using System.Collections.Generic;
using System;
using System.Text;

namespace WordFinder5000.Core
{
    public interface IFilerParser
    {
        List<string> Parse(string content);
    }

    public class FileParser : IFilerParser
    {
       
        public List<string> Parse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content is empty!", nameof(content));
            return CreateList(content);
        }

        private List<string> CreateList(string content)
        {
            var sb = new StringBuilder();
            var list = new List<string>();
            foreach (var ch in content)
            {
                if (IsValidCharacter(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    if (sb.Length <= 0) continue;
                    list.Add(sb.ToString());
                    sb = new StringBuilder();
                }
            }

            if (sb.Length > 0)
                list.Add(sb.ToString());
            return list;
        }

        private bool IsValidCharacter(char ch)
        {
            return char.IsLetter(ch) || SpecialCharacters.Apostrophes.Contains(ch);
        }
    }
}