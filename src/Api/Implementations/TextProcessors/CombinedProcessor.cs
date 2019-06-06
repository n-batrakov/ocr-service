using System.Collections.Generic;
using System.Linq;
using OcrService.Core;

namespace OcrService.Implementations.TextProcessors
{
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