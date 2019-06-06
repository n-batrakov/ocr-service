using System;
using OcrService.Implementations.TextProcessors;
using Xunit;
using Xunit.Sdk;

namespace UnitTests
{
    public class PerWordProcessorTests
    {
        private static PerWordProcessor Sut(Func<string, string> fn) =>
            new PerWordProcessor(new[] {new CallbackProcessor(fn)});

        [Fact]
        public void CanProcessEmptyString()
        {
            var sut = Sut(x => throw new TestException());
            
            var expected = "";

            var actual = sut.Process("");
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void CanProcessSingleWord()
        {
            var expected = "test";
            
            var actual = Sut(x => x).Process("test");
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessTextWithNoWords()
        {
            var sut = Sut(_ => throw new TestException());
            
            var expected = ";,,#!";
            var actual = sut.Process(expected);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessSentences()
        {
            var sut = Sut(_ => "test");

            var expected = "test, test, test";

            var actual = sut.Process("one, two, three");
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessSentencesNonWordStart()
        {
            var sut = Sut(_ => "test");

            var expected = "%# test,test,test";

            var actual = sut.Process("%# One,Two,Three");
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanProcessSentencesNonWordEnd()
        {
            var sut = Sut(_ => "test");

            var expected = "test,test,test %#";

            var actual = sut.Process("One,Two,Three %#");
            
            Assert.Equal(expected, actual);
        }

        private class TestException : XunitException
        {
            public TestException(): base("Processor was not expected to be called.")
            {
                
            }
        }
    }
}