using ITExpert.OcrService.Implementations.SymSpellTextProcessor;
using ITExpert.OcrService.Implementations.TextProcessors;
using Xunit;

namespace UnitTests
{
    public class RemoveEmptyLinesProcessorTests
    {
        private static RemoveEmptyLinesProcessor Sut() => new RemoveEmptyLinesProcessor();

        private static RemoveEmptyLinesProcessor Sut(NormalizeLineEndingsStrategy strategy) =>
            new RemoveEmptyLinesProcessor(new RemoveEmptyLinesOptions {NormalizeLineEndings = strategy});
        
        [Fact]
        public void CanProcessEmptyString()
        {
            var expected = "";
            
            var actual = Sut().Process("");
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanRemoveEmptyLinesLfEndings()
        {
            var expected = "one\ntwo\nthree";

            var actual = Sut(NormalizeLineEndingsStrategy.Lf).Process("\none\n\r\n\ntwo\nthree\r\n\r\n");
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanRemoveEmptyLinesCrlfEndings()
        {
            var expected = "one\r\ntwo\r\nthree";

            var actual = Sut(NormalizeLineEndingsStrategy.Crlf).Process("\none\n\r\n\ntwo\nthree\r\n\r\n");
            
            Assert.Equal(expected, actual);
        }
    }
}