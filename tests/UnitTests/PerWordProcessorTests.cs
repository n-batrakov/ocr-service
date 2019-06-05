using System;
using ITExpert.OcrService.Core;
using Xunit;
using Xunit.Sdk;

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

    public class PerWordProcessorTests
    {
        private PerWordProcessor Sut(Func<string, string> fn) =>
            new PerWordProcessor(new[] {new CallbackProcessor(fn)});
        
        [Fact]
        public void CanProcessSingleWord()
        {
            var expected = "Foo";
            
            var actual = Sut(x => x).Process(expected);
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessTextWithNoWords()
        {
            var sut = Sut(_ => throw new XunitException("Processor was not expected to be called."));
            
            var expected = ";,,#!";
            var actual = sut.Process(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessMultiWordText()
        {
            var sut = Sut(_ => "test");

            var expected = "test, test. test!";

            var actual = sut.Process("One, two. Three!");
            
            Assert.Equal(expected, actual);
        }
    }
}