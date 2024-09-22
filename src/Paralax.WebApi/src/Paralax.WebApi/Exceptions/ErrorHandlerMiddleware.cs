using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetJSON;

namespace Paralax.WebApi.Exceptions
{
    internal sealed class ErrorHandlerMiddleware : IMiddleware
    {
        private readonly IExceptionToResponseMapper _exceptionToResponseMapper;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(IExceptionToResponseMapper exceptionToResponseMapper,
            ILogger<ErrorHandlerMiddleware> logger)
        {
            _exceptionToResponseMapper = exceptionToResponseMapper;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while processing the request.");
                await HandleErrorAsync(context, exception);  // Handle the error when caught
            }
        }

        private async Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var exceptionResponse = _exceptionToResponseMapper.Map(exception);

            context.Response.StatusCode = (int)(exceptionResponse?.StatusCode ?? HttpStatusCode.BadRequest);

            var response = exceptionResponse?.Response;

            if (response is null)
            {
                await context.Response.WriteAsync(string.Empty);
                return;
            }

            context.Response.ContentType = "application/json";

            // Use NetJSON to serialize the response
            var jsonResponse = NetJSON.NetJSON.Serialize(response);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
