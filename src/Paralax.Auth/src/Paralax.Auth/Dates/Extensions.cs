using System;

namespace Paralax.Auth.Dates
{
    internal static class Extensions
    {
        // Converts DateTime to Unix timestamp (seconds since 1 January 1970)
        public static long ToTimestamp(this DateTime dateTime) 
            => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

        // Converts Unix timestamp to DateTime (UTC)
        public static DateTime FromTimestamp(this long timestamp)
            => DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

        // Converts DateTime to Unix timestamp in milliseconds
        public static long ToTimestampMilliseconds(this DateTime dateTime) 
            => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        // Converts Unix timestamp in milliseconds to DateTime (UTC)
        public static DateTime FromTimestampMilliseconds(this long timestampMilliseconds)
            => DateTimeOffset.FromUnixTimeMilliseconds(timestampMilliseconds).UtcDateTime;
    }
}
