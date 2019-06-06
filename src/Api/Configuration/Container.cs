using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OcrService.Core;
using OcrService.Implementations;
using OcrService.Implementations.ImageProcessors;
using OcrService.Implementations.TextProcessors;

namespace OcrService.Configuration
{
    public static class Container
    {
        // ReSharper disable once UnusedParameter.Global
        public static IServiceCollection AddAppDependencies(this IServiceCollection services, IConfiguration config)
        {
            AddOcr(services);
            AddPreProcessing(services);
            AddPostProcessing(services);
            
            services.AddSingleton(JsonSerializer.Create(JsonConfiguration.Instance));
            services.AddHttpClient();
            
            return services;
        }

        private static void AddOcr(IServiceCollection services)
        {
            // TODO: Check tesseract binary
            //var tempDir = "../../tmp";
            //Directory.CreateDirectory(tempDir);
            //services.AddSingleton<IOcrClient>(new TesseractOcrClient(new TesseractOptions(tempDir, "tesseract")));
            
            services.AddSingleton(FakeOcr.Instance);
        }
        
        private static void AddPreProcessing(IServiceCollection services)
        {
            services.AddSingleton(NullImageProcessor.Instance);
        }
        
        private static void AddPostProcessing(IServiceCollection services)
        {
            var symSpell = new SymSpell();
            Console.Out.WriteLine("Loading SymSpell dictionary...");
            {
                symSpell.LoadDictionary("../../ru.dict", termIndex: 0, countIndex: 1);
            }
            Console.Out.WriteLine("SymSpell initialized!");

            var postProcessor = new CombinedProcessor(new ITextPostProcessor[]
            {
                new RemoveEmptyLinesProcessor(new RemoveEmptyLinesOptions
                {
                    NormalizeLineEndings = NormalizeLineEndingsStrategy.Lf
                }),
                
                new PerWordProcessor(new ITextPostProcessor[]
                {
                    new SymSpellProcessor(symSpell, 1, Enumerable.Empty<string>())
                })
            });
            
            services.AddSingleton(postProcessor);
        }
    }
}