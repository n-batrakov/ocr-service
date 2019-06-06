using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcrService.Api;
using OcrService.Middleware.ExceptionHandler;

namespace OcrService.Configuration
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAppDependencies(Configuration);
            services
                .AddMvcCore()
                .AddJsonFormatters(JsonConfiguration.Configure)
                .AddDataAnnotations()
                .AddFormatterMappings()
                .AddApiExplorer()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseApiExceptionSerializer(options =>
            {
                options.ShowStackTrace = env.IsDevelopment();
                options.ConfigureException<UnsupportedMediaTypeException>(c => c.HttpStatusCode = (int)HttpStatusCode.UnsupportedMediaType);
                options.ConfigureException<BadRequestException>(c => c.HttpStatusCode = (int)HttpStatusCode.BadRequest);
            });
            app.UseMvc();
        }
    }
}
