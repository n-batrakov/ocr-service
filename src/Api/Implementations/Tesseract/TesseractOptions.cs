using System;
using System.Collections.Generic;
using System.Linq;

namespace OcrService.Implementations.Tesseract
{
    public class TesseractOptions
    {
        public IReadOnlyCollection<string> ImageLanguages { get; }
        public string WorkingDirectory { get; }
        public string TesseractBinary { get; }
        public TesseractPageSegmentation Psm { get; set; } = TesseractPageSegmentation.FullyAutomaticNoOsd;

        public TesseractOptions(string workingDirectory, string tesseractBinary) : this(workingDirectory,
            tesseractBinary, new[] {"rus", "eng"})
        {
        }

        public TesseractOptions(string workingDirectory, string tesseractBinary, IEnumerable<string> imageLanguages)
        {
            ImageLanguages = imageLanguages?.ToArray() ?? throw new ArgumentNullException(nameof(imageLanguages));
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            TesseractBinary = tesseractBinary ?? throw new ArgumentNullException(nameof(tesseractBinary));
        }
    }
}