using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ITExpert.OcrService.Core
{
    public enum OcrStatus
    {
        Success,
        Error
    }
    
    public class OcrResult
    {
        public OcrStatus Status { get; }
        public string Text { get; }
        public string Error { get; }

        private OcrResult(OcrStatus status, string text, string error)
        {
            Status = status;
            Text = text;
            Error = error;
        }

        public static OcrResult Success(string text) => new OcrResult(OcrStatus.Success, text, null);
        public static OcrResult Fail(string error) => new OcrResult(OcrStatus.Error, null, error);

    }
    
    public interface IOcrClient
    {
        Task<OcrResult> RecognizeAsync(Stream image, CancellationToken token);
    }
}