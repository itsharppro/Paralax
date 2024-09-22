using System.Net;

namespace Paralax.WebApi.Exceptions
{
    public class ExceptionResponse
    {
        /// <summary>
        /// The response object containing error details or messages.
        /// </summary>
        public object Response { get; }

        /// <summary>
        /// The HTTP status code associated with this exception response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Constructor for creating an instance of ExceptionResponse.
        /// </summary>
        /// <param name="response">The response object, typically containing error messages or details.</param>
        /// <param name="statusCode">The HTTP status code indicating the type of error.</param>
        public ExceptionResponse(object response, HttpStatusCode statusCode)
        {
            Response = response;
            StatusCode = statusCode;
        }

        /// <summary>
        /// A utility method to create a standard error response.
        /// </summary>
        /// <param name="message">Error message to return in the response.</param>
        /// <param name="statusCode">HTTP status code of the error.</param>
        /// <returns>A structured exception response with the message and status code.</returns>
        public static ExceptionResponse Create(string message, HttpStatusCode statusCode)
        {
            var response = new
            {
                error = true,
                message = message
            };

            return new ExceptionResponse(response, statusCode);
        }
    }
}
