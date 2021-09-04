using System;
using System.Collections.Generic;

namespace WordFinder5000.Core
{
    public class HashSetIgnoreCase : HashSet<string>
    {
        public HashSetIgnoreCase() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public HashSetIgnoreCase(IEnumerable<string> enumerable) : base(enumerable, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}