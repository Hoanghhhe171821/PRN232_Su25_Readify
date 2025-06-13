using PRN232_Su25_Readify_WebAPI.Exceptions;
using System.Net;
using System.Text.Json;

namespace PRN232_Su25_Readify_WebAPI.Middlewares
{
    public class ExceptionMid
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMid> _logger;

        public ExceptionMid(RequestDelegate next, ILogger<ExceptionMid> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, cannot write error response.");
                return Task.CompletedTask;
            }

            context.Response.ContentType = "application/json";
            HttpStatusCode statusCode;
            object errorResponse;

            switch (exception)
            {
                case BRException badRequest:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = BuildError(badRequest.Message, context.TraceIdentifier);
                    break;

                case NFoundEx notFound:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = BuildError(notFound.Message, context.TraceIdentifier);
                    break;

                case ConflictEx conflict:
                    statusCode = HttpStatusCode.Conflict;
                    errorResponse = BuildError(conflict.Message, context.TraceIdentifier);
                    break;

                case UnauthorEx unauthorized:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorResponse = BuildError(unauthorized.Message, context.TraceIdentifier);
                    break;

                case ValidationEx validation:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        message = "Validation Failed",
                        errors = validation.Errors,
                        traceId = context.TraceIdentifier
                    };
                    break;

                case ArgumentNullException argNullEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        message = argNullEx.Message,
                        paramName = argNullEx.ParamName,
                        traceId = context.TraceIdentifier
                    };
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = BuildError(argEx.Message, context.TraceIdentifier);
                    break;

                case InvalidOperationException invalidOp:
                    statusCode = HttpStatusCode.Conflict;
                    errorResponse = BuildError(invalidOp.Message, context.TraceIdentifier);
                    break;

                case NotImplementedException notImpl:
                    statusCode = HttpStatusCode.NotImplemented;
                    errorResponse = BuildError("This functionality is not implemented.", context.TraceIdentifier);
                    break;

                case KeyNotFoundException keyNotFound:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = BuildError(keyNotFound.Message, context.TraceIdentifier);
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse = BuildError("An unexpected error occurred.", context.TraceIdentifier);
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

        private object BuildError(string message, string traceId)
        {
            return new
            {
                message,
                traceId
            };
        }
    }
}
