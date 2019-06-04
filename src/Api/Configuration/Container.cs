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
            services.AddSingleton<IOcrClient>(new TesseractOcrClient(new TesseractOptions(".", "tesseract")));

            services.AddHttpClient();
            
            return services;
        }
    }
}