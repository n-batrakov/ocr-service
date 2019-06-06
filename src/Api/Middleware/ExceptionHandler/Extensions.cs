using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OcrService.Middleware.ExceptionHandler
{
    public static class UseExceptionHandlerExtension
    {
        public static IApplicationBuilder UseApiExceptionSerializer(this IApplicationBuilder app) =>
            UseApiExceptionSerializer(app, _ => { });

        public static IApplicationBuilder UseApiExceptionSerializer(this IApplicationBuilder app,
            Action<ExceptionHandlerOptions> configure)
        {
            var options = new ExceptionHandlerOptions();
            configure(options);
            return app.UseMiddleware(typeof(ExceptionHandlerMiddleware), options);
        }
    }
    
    internal class ExceptionHandlerMiddleware
    {
        private RequestDelegate Next { get; }
        private ILogger Logger { get; }
        private ExceptionHandlerOptions Options { get; }

        public ExceptionHandlerMiddleware(RequestDelegate next, ExceptionHandlerOptions options)
        {
            Next = next;
            Options = options;
        }

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger logger, ExceptionHandlerOptions options)
        {
            Next = next;
            Logger = logger;
            Options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            Exception exception;
            try
            {
                await Next.Invoke(context);
                return;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "An exception has occurred");
                var hasExceptionConfig =
                        Options.ConfigurationPerExceptionType.TryGetValue(ex.GetType(), out var exceptionOptions);
                var statusCode = hasExceptionConfig ? exceptionOptions.HttpStatusCode : 500;
                context.Response.StatusCode = statusCode;
                exception = ex;
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/json";

            var response = ErrorResponseBuilder.Build(exception, Options);
            var json = JsonConvert.SerializeObject(response,
                                                   new JsonSerializerSettings
                                                   {
                                                       ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                                       NullValueHandling = NullValueHandling.Ignore
                                                   });
            await context.Response.WriteAsync(json);
        }
    }
}