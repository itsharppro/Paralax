using Paralax.Common;
using Microsoft.Extensions.Logging;

namespace Paralax.gRPC.Protobuf.Utilities
{
    public static class GrpcUtility
    {
        public static Status CreateSuccessStatus(string message)
        {
            return new Status
            {
                Success = true,
                Message = message,
                Code = 200,
                Details = "Operation completed successfully"
            };
        }

        public static Status CreateErrorStatus(string message, int errorCode, string details, ILogger? logger = null)
        {
            logger?.LogError($"Error: {message}, Code: {errorCode}, Details: {details}");
            return new Status
            {
                Success = false,
                Message = message,
                Code = errorCode,
                Details = details
            };
        }

        public static Error CreateError(int code, string message, List<string> details, ILogger? logger = null)
        {
            logger?.LogError($"Error: {message}, Code: {code}, Details: {string.Join(", ", details)}");
            return new Error
            {
                Code = code,
                Message = message,
                Details = { details }
            };
        }
    }
}
