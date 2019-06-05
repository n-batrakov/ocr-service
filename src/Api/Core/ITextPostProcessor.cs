using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ITExpert.OcrService.Core
{
    public interface ITextPostProcessor
    {
        string Process(string text);
    }

    public class NullPostProcessor : ITextPostProcessor
    {
        public static readonly ITextPostProcessor Instance = new NullPostProcessor();
        
        private NullPostProcessor()
        {
        }
        
        public string Process(string text) => text;
    }

    public static class PostProcessor
    {
        public static ITextPostProcessor Combine(params ITextPostProcessor[] processors) =>
            new CombinedProcessor(processors);

        public static ITextPostProcessor WordProcessors(params ITextPostProcessor[] processors) =>
            new PerWordProcessor(processors);
    }
    
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
    
    public class CombinedProcessor : ITextPostProcessor
    {
        private IReadOnlyCollection<ITextPostProcessor> Processors { get; }
            
        public CombinedProcessor(IReadOnlyCollection<ITextPostProcessor> processors) => 
            Processors = processors;
            
        public string Process(string text) => 
            Processors.Aggregate(text, Reducer);
            
        private static string Reducer(string s, ITextPostProcessor p) => p.Process(s);
    }
}