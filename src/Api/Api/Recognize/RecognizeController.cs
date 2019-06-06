using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OcrService.Core;

namespace OcrService.Api.Recognize
{
    public class NetImageOptions
    {
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
    public class RecognizeFromUrlController : ControllerBase
    {
        private IImagePreProcessor PreProcessor { get; }
        private IOcrClient OcrClient { get; }
        private ITextPostProcessor PostProcessor { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        private JsonSerializer Serializer { get; }

        public RecognizeFromUrlController(
            IImagePreProcessor preProcessor,
            IOcrClient ocrClient,
            ITextPostProcessor postProcessor,
            IHttpClientFactory httpClientFactory,
            JsonSerializer serializer)
        {
            PreProcessor = preProcessor;
            OcrClient = ocrClient;
            PostProcessor = postProcessor;
            HttpClientFactory = httpClientFactory;
            Serializer = serializer;
        }

        [HttpPost("v1/recognize")]
        [Consumes("application/octet-stream", "application/json", "multipart/form-data")] 
        [Produces("application/json"), ProducesResponseType(200)]
        public async Task<RecognizeResponse> ExecuteAsync(CancellationToken token)
        {
            var image = await GetFileAsync(token);
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

        private Task<Stream> GetFileAsync(CancellationToken token)
        {
            var contentType = MediaTypeHeaderValue.Parse(Request.ContentType);

            switch (contentType.MediaType)
            {
                case "application/octet-stream":
                    return Task.FromResult(Request.Body);
                case "multipart/form-data":
                    return HandleFormInputAsync(token);
                case "application/json":
                    return HandleJsonInputAsync(token);
                default:
                    throw new UnsupportedMediaTypeException(
                        $"Content-Type header MUST be set to 'application/octet-stream', 'multipart/form-data' or 'application/json', but '{contentType}' received.",
                        contentType);
            }
        }

        private async Task<Stream> HandleFormInputAsync(CancellationToken token)
        {
            var form = await Request.ReadFormAsync(token).ConfigureAwait(false);

            if (form.Files.Count == 0 || form.Files.Count > 1)
            {
                throw new BadRequestException($"Form-Data MUST contain exactly one image file, but got {form.Files.Count}.");
            }
            
            return form.Files.First().OpenReadStream();
        }
        
        private async Task<Stream> HandleJsonInputAsync(CancellationToken token)
        {
            using (var sw = new StreamReader(Request.Body))
            {
                var options = (NetImageOptions)Serializer.Deserialize(sw, typeof(NetImageOptions));

                if (string.IsNullOrEmpty(options.Uri))
                {
                    throw new BadRequestException("application/json payload MUST contain 'url' property.");
                }
                
                return await ReadImageFromUrl(options, token).ConfigureAwait(false);
            }
        }

        private async Task<Stream> ReadImageFromUrl(NetImageOptions options, CancellationToken token)
        {
            var client = HttpClientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(options.Uri),
                Method = new HttpMethod(options.Method),
                Content = options.Body == null ? null : new StringContent(options.Body, Encoding.UTF8)
            };
            if (options.Headers != null)
            {
                CopyHeaders(options.Headers, request.Headers);                
            }
            
            
            var response = await client.SendAsync(request, token);

            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType;
            if (contentType.MediaType.StartsWith("image"))
            {
                return await response.Content.ReadAsStreamAsync();                
            }
            else
            {
                throw new UnsupportedMediaTypeException($"Provided URL must return image/* content, but {contentType} received.", contentType);
            }
            
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