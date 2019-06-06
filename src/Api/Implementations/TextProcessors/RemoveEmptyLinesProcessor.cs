using System;
using System.Text.RegularExpressions;
using OcrService.Core;

namespace OcrService.Implementations.TextProcessors
{
    public enum NormalizeLineEndingsStrategy
    {
        Lf,
        Crlf
    }
    
    public class RemoveEmptyLinesOptions
    {
        public NormalizeLineEndingsStrategy NormalizeLineEndings { get; set; }

        public static readonly RemoveEmptyLinesOptions Default = new RemoveEmptyLinesOptions
        {
            NormalizeLineEndings = NormalizeLineEndingsStrategy.Lf
        };
    }
    
    public class RemoveEmptyLinesProcessor : ITextPostProcessor
    {
        private RemoveEmptyLinesOptions Options { get; }
        private static readonly Regex Regex = new Regex("(\r?\n)+", RegexOptions.Compiled | RegexOptions.Multiline);

        public RemoveEmptyLinesProcessor()
        {
            Options = RemoveEmptyLinesOptions.Default;
        }
        
        public RemoveEmptyLinesProcessor(RemoveEmptyLinesOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        public string Process(string text)
        {
            switch (Options.NormalizeLineEndings)
            {
                case NormalizeLineEndingsStrategy.Lf:
                    return TrimLf(Regex.Replace(text, "\n"));
                case NormalizeLineEndingsStrategy.Crlf:
                    return TrimCrlf(Regex.Replace(text, "\r\n"));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string TrimCrlf(string text)
        {
            if (text.StartsWith("\r\n"))
            {
                text = text.Substring(2);
            }

            if (text.EndsWith("\r\n"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        private static string TrimLf(string text)
        {
            if (text.StartsWith("\n"))
            {
                text = text.Substring(1);
            }

            if (text.EndsWith("\n"))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }
    }
}