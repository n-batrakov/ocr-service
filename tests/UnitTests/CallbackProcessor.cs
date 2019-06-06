using System;
using ITExpert.OcrService.Core;

namespace UnitTests
{
    public class CallbackProcessor : ITextPostProcessor
    {
        private Func<string, string> Callback { get; }
        
        public CallbackProcessor(Func<string, string> callback)
        {
            Callback = callback;
        }
        
        public string Process(string text)
        {
            return Callback(text);
        }
    }
}