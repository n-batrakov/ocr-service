using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using ITExpert.OcrService.Core;

namespace ITExpert.OcrService.Implementations.Tesseract
{
    public class TesseractOcrClient : IOcrClient
    {
        private string WorkingDirectory { get; }
        private string TesseractBinary { get; }
        private IReadOnlyCollection<string> ImageLanguages { get; }
        private TesseractPageSegmentation Psm { get; }

        public TesseractOcrClient(TesseractOptions options)
        {
            WorkingDirectory = options.WorkingDirectory;
            TesseractBinary = options.TesseractBinary;
            ImageLanguages = options.ImageLanguages;
            Psm = options.Psm;
        }
        
        
        public async Task<OcrResult> RecognizeAsync(Stream image, CancellationToken token)
        {
            var inputFile = await WriteFileAsync(image, token);

            var result = await Cli.Wrap(TesseractBinary)
                .SetArguments(FormatCliArguments(inputFile))
                .SetStandardOutputEncoding(Encoding.UTF8)
                .SetWorkingDirectory(WorkingDirectory)
                .SetCancellationToken(token)
                .ExecuteAsync();

            if (result.ExitCode == 0)
            {
                var text = result.StandardOutput;
                return OcrResult.Success(text);
            }
            else
            {
                var error = result.StandardError;
                return OcrResult.Fail(error);
            }
        }

        private string FormatCliArguments(string inputPath)
        {
            var psm = (int) Psm;
            var lang = string.Join("+", ImageLanguages);
            return $"{inputPath} stdout --oem 1 -l {lang} --psm {psm}";
        }

        private async Task<string> WriteFileAsync(Stream image, CancellationToken token)
        {
            if (!image.CanRead)
            {
                throw new ArgumentException("Stream is not readable.");
            }
            
            var fileName = Guid.NewGuid().ToString("N");
            var filePath = Path.Join(WorkingDirectory, fileName);

            using (var fileStream = File.Create(filePath))
            {
                if (image.CanSeek)
                {
                    image.Seek(0, SeekOrigin.Begin);                    
                }
                
                await image.CopyToAsync(fileStream, token);
            }

            return fileName;
        }
    }
}