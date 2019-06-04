using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ITExpert.OcrService.Core
{
    public interface IImagePreProcessor
    {
        Stream Process(Stream stream);
    }

    public class NullImageProcessor : IImagePreProcessor
    {
        public static readonly IImagePreProcessor Instance = new NullImageProcessor();
        
        private NullImageProcessor()
        {
        }
        
        public Stream Process(Stream stream) => stream;
    }

    public static class ImageProcessor
    {
        public static IImagePreProcessor Combine(params IImagePreProcessor[] processors)
        {
            return new CombinedImageProcessor(processors);
        }
        
        private class CombinedImageProcessor : IImagePreProcessor
        {
            private IReadOnlyCollection<IImagePreProcessor> Processors { get; }

            public CombinedImageProcessor(IReadOnlyCollection<IImagePreProcessor> processors) =>
                Processors = processors;

            public Stream Process(Stream stream) => 
                Processors.Aggregate(stream, Reducer);

            private static Stream Reducer(Stream acc, IImagePreProcessor x) => 
                x.Process(acc);
        }
    }
}