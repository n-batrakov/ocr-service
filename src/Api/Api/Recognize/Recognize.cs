using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ITExpert.OcrService.Core;

namespace ITExpert.OcrService.Api.Recognize
{
    public class RecognizeFromUrlRequest
    {
        [Required]
        public string Uri { get; set; }

        public string Method { get; set; } = "GET";
        public IDictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
    }
    
    public class RecognizeResponse
    {
        public string Text { get; }

        public RecognizeResponse(string text)
        {
            Text = text;
        }
    }
    
    [ApiController]
    public class RecognizeFromUrlController : Controller
    {
        private IImagePreProcessor PreProcessor { get; }
        private IOcrClient OcrClient { get; }
        private ITextPostProcessor PostProcessor { get; }
        private IHttpClientFactory HttpClientFactory { get; }

        public RecognizeFromUrlController(
            IImagePreProcessor preProcessor,
            IOcrClient ocrClient,
            ITextPostProcessor postProcessor,
            IHttpClientFactory httpClientFactory)
        {
            PreProcessor = preProcessor;
            OcrClient = ocrClient;
            PostProcessor = postProcessor;
            HttpClientFactory = httpClientFactory;
        }

        [HttpPost("v1/recognize")]
        public async Task<RecognizeResponse> ExecuteAsync([FromBody] RecognizeFromUrlRequest request, CancellationToken token)
        {
            var image = await ReadImageFromUrl(request, token).ConfigureAwait(false);
            image = PreProcessor.Process(image);
            var ocrResult = await OcrClient.RecognizeAsync(image, token).ConfigureAwait(false);

            switch (ocrResult.Status)
            {
                case OcrStatus.Success:
                    var text = PostProcessor.Process(ocrResult.Text);
                    return new RecognizeResponse(text);
                case OcrStatus.Error:
                    throw new Exception(ocrResult.Error);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<Stream> ReadImageFromUrl(RecognizeFromUrlRequest options, CancellationToken token)
        {
            var client = HttpClientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(options.Uri),
                Method = new HttpMethod(options.Method),
                Content = options.Body == null ? null : new StringContent(options.Body, Encoding.UTF8),
            };
            if (options.Headers != null)
            {
                CopyHeaders(options.Headers, request.Headers);                
            }
            
            
            var response = await client.SendAsync(request, token);

            response.EnsureSuccessStatusCode();
            
            // TODO: handle response by content-type
            
            throw new NotImplementedException();
        }

        private static void CopyHeaders(IDictionary<string, string> source, HttpHeaders destination)
        {
            foreach (var (key, value) in source)
            {
                destination.TryAddWithoutValidation(key, value);
            }
        }
    }
}