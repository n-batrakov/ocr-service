using System.Collections.Generic;
using System.Linq;

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

        private class CombinedProcessor : ITextPostProcessor
        {
            private IReadOnlyCollection<ITextPostProcessor> Processors { get; }
            
            public CombinedProcessor(IReadOnlyCollection<ITextPostProcessor> processors) => 
                Processors = processors;
            
            public string Process(string text) => 
                Processors.Aggregate(text, Reducer);
            
            private static string Reducer(string s, ITextPostProcessor p) => p.Process(s);
        }
    }
}