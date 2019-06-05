using System;
using System.Collections.Generic;
using System.Linq;
using ITExpert.OcrService.Core;

namespace ITExpert.OcrService.Implementations.SymSpellTextProcessor
{
    public class CleanupTextProcessor : ITextPostProcessor
    {
        public string Process(string text)
        {
            return text;
        }
    }

    public class SymSpellProcessor : ITextPostProcessor
    {
        private HashSet<string> IgnoreWords { get; }
        private SymSpell Speller { get; }
        private int MaxDistance { get; }

        public SymSpellProcessor(SymSpell speller, int maxDistance, IEnumerable<string> ignoreWords)
        {
            Speller = speller;
            MaxDistance = maxDistance;
            IgnoreWords = new HashSet<string>(ignoreWords, StringComparer.InvariantCultureIgnoreCase);
        }
        
        public string Process(string word)
        {
            if (word.Length < 2)
            {
                return word;
            }

            if (word.ToCharArray().Any(char.IsDigit))
            {
                return word;
            }

            if (IgnoreWords.Contains(word))
            {
                return word;
            }

            var suggestions = Speller.Lookup(word, SymSpell.Verbosity.Closest, MaxDistance);
            return suggestions.Count == 0 ? word : suggestions[0].term;
        }
    }
}