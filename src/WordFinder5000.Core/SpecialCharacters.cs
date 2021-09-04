using System.Collections.Generic;

namespace WordFinder5000.Core
{
    public static class SpecialCharacters
    {
        public static List<char> Apostrophes => new List<char> { '\'', '’' }; // single quote and apostrophe 

        public static List<string> NotAllowed => new List<string>
        {
            "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "+", "=", "~", "`", "\"", "{", "}", "[",
            "]", "|", "\\", "/", ":", ";", ",", ".", "?", "<", ">"
        };
    }
}