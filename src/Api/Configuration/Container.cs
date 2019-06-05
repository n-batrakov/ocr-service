using System.IO;
using ITExpert.OcrService.Core;
using ITExpert.OcrService.Tesseract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITExpert.OcrService.Configuration
{
    public static class Container
    {
        // ReSharper disable once UnusedParameter.Global
        public static IServiceCollection AddAppDependencies(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(NullImageProcessor.Instance);
            services.AddSingleton(NullPostProcessor.Instance);

            var tempDir = "../../tmp";
            Directory.CreateDirectory(tempDir);
            services.AddSingleton<IOcrClient>(new TesseractOcrClient(new TesseractOptions(tempDir, "tesseract")));

            services.AddHttpClient();
            
            return services;
        }
    }
}