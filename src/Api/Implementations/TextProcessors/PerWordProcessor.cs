using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OcrService.Core;

namespace OcrService.Implementations.TextProcessors
{
    public class PerWordProcessor : ITextPostProcessor 
    {
        private static readonly Regex WordRegex = new Regex(@"\b[\w']+\b");
        private IReadOnlyCollection<ITextPostProcessor> WordProcessors { get; }

        public PerWordProcessor(IReadOnlyCollection<ITextPostProcessor> wordProcessors)
        {
            WordProcessors = wordProcessors;
        }

        public string Process(string text)
        {
            var matches = WordRegex.Matches(text);

            return matches.Count == 0 ? text : string.Join("", IterateMatches(text, matches));
        }

        private IEnumerable<string> IterateMatches(string text, MatchCollection matches)
        {
            var currentIndex = 0;
            
            foreach (Match match in matches)
            {
                var textStart = text.Substring(currentIndex, match.Index - currentIndex);

                if (!string.IsNullOrEmpty(textStart))
                {
                    yield return textStart;                    
                }
                
                
                var word = TrimSuffix(match.Value);
                var newWord = WordProcessors.Aggregate(word, Reducer);
                yield return newWord;

                currentIndex = match.Index + word.Length;
            }

            if (currentIndex < text.Length)
            {
                yield return text.Substring(currentIndex);
            }
        }
        
        private static string Reducer(string word, ITextPostProcessor processor) =>
            processor.Process(word);

        private static string TrimSuffix(string word)
        {
            var idx = word.IndexOf('\'');
            return idx == -1 ? word : word.Substring(0, idx);
        }
    }
}