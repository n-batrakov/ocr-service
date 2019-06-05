using System;
using System.Collections.Generic;
using System.Linq;

namespace ITExpert.OcrService.Middleware.ExceptionHandler
{
    /// <summary>
    /// Exception specific configuration options.
    /// </summary>
    public class ExceptionHandlerExceptionOptions
    {
        /// <summary>
        /// HTTP Status code set for response if the exception occurs. 
        /// 500 is the default.
        /// </summary>
        public int HttpStatusCode { get; set; } = 500;

        /// <summary>
        /// Exception code displayed to the client. 
        /// If value is null exception type name will be used.
        /// </summary>
        public string ExceptionCode { get; set; }

        /// <summary>
        /// Exception message displayed to the client. 
        /// If value is null exception message will be used.
        /// </summary>
        public string ExceptionMessage { get; set; }
    }
    
    /// <summary>
    /// ExceptionSerializer options.
    /// </summary>
    public class ExceptionHandlerOptions
    {
        /// <summary>
        /// True if ExceptionSerializer should log every exception.
        /// </summary>
        public bool LogExceptions { get; set; } = true;

        /// <summary>
        /// Logger name used to log exceptions if LogException set to True.
        /// </summary>
        public string LoggerName { get; set; } = "ExceptionMiddleware";

        /// <summary>
        /// Exception specific options.
        /// </summary>
        public IReadOnlyDictionary<Type, ExceptionHandlerExceptionOptions> ConfigurationPerExceptionType =>
                ConfigurationPerExceptionTypeSource;

        private Dictionary<Type, ExceptionHandlerExceptionOptions> ConfigurationPerExceptionTypeSource { get; } =
            new Dictionary<Type, ExceptionHandlerExceptionOptions>();

        /// <summary>
        /// True if StackTrace should be sent to client. In production this options should be set to False.
        /// </summary>
        public bool ShowStackTrace { get; set; }

        internal ExceptionHandlerOptions()
        {
        }

        /// <summary>
        /// Adds Exception specific configuration.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="configure">Configuration callback.</param>
        public void ConfigureException<TException>(Action<ExceptionHandlerExceptionOptions> configure)
            where TException : Exception
        {
            var options = new ExceptionHandlerExceptionOptions();
            configure(options);
            ConfigurationPerExceptionTypeSource.Add(typeof(TException), options);
        }

        /// <summary>
        /// Removes Exception specific configuration.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        public void UnconfigureException<TException>() where TException : Exception
        {
            ConfigurationPerExceptionTypeSource.Remove(typeof(TException));
        }
    }
    
    
    
    internal class ErrorResponse
    {
        public ErrorResponseBody Error { get; }

        public string StackTrace { get; }

        public ErrorResponse(ErrorResponseBody error, string stackTrace = null)
        {
            Error = error;
            StackTrace = stackTrace;
        }
    }
    
    internal class ErrorResponseBody
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorResponseBody[] Details { get; }

        public InnerError Innererror { get; }

        public ErrorResponseBody(string code,
            string message,
            ErrorResponseBody[] details = null,
            InnerError innererror = null)
        {
            Code = code;
            Message = message;
            Details = details;
            Innererror = innererror;
        }
    }
    
    internal static class ErrorResponseBuilder
    {
        public static ErrorResponse Build(Exception exception, ExceptionHandlerOptions options)
        {
            var body = CreateBody(exception, options);
            var stackTrace = options.ShowStackTrace ? exception.StackTrace : null;
            return new ErrorResponse(body, stackTrace);
        }

        private static ErrorResponseBody CreateBody(Exception exception, ExceptionHandlerOptions options)
        {
            options.ConfigurationPerExceptionType.TryGetValue(exception.GetType(), out var exceptionOptions);
            var errorCode = exceptionOptions?.ExceptionCode ?? GetErrorCode(exception);
            var message = exceptionOptions?.ExceptionMessage ?? exception.Message;
            var details = GetDetails(exception, options);
            var inner = GetInner(exception);
            return new ErrorResponseBody(errorCode, message, details, inner);
        }

        private static string GetErrorCode(Exception e)
        {
            var name = e.GetType().Name;
            if (name == "Exception")
            {
                return "Generic";
            }
            if (name.EndsWith("Exception", StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(0, name.Length - 9);
            }

            return name;
        }

        private static ErrorResponseBody[] GetDetails(Exception e, ExceptionHandlerOptions options)
        {
            if (e is AggregateException aggregate && aggregate.InnerExceptions.Count > 0)
            {
                return aggregate.InnerExceptions.Select(x => CreateBody(x, options)).ToArray();
            }

            return null;
        }

        private static InnerError GetInner(Exception e) =>
                e.InnerException == null || e is AggregateException ae && ae.InnerExceptions.Count > 0
                    ? null
                    : new InnerError(GetErrorCode(e.InnerException), GetInner(e.InnerException));
    }
    
    
    
    internal class InnerError
    {
        public string Code { get; }

        public InnerError Innererror { get; }

        public InnerError(string code, InnerError innerError)
        {
            Code = code;
            Innererror = innerError;
        }
    }
}