using System.IO;
using OcrService.Core;

namespace OcrService.Implementations.ImageProcessors
{
    public class NullImageProcessor : IImagePreProcessor
    {
        public static readonly IImagePreProcessor Instance = new NullImageProcessor();
        
        private NullImageProcessor()
        {
        }
        
        public Stream Process(Stream stream) => stream;
    }
}