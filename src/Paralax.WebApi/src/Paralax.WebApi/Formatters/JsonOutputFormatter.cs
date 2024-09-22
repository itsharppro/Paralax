using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using NetJSON;

namespace Paralax.WebApi.Formatters
{
    internal class JsonOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return true;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is null)
            {
                return;
            }

            context.HttpContext.Response.ContentType = "application/json";

            if (context.Object is string json)
            {
                await context.HttpContext.Response.WriteAsync(json);
                return;
            }

            var serializedJson = NetJSON.NetJSON.Serialize(context.Object);
            await context.HttpContext.Response.WriteAsync(serializedJson);
        }
    }
}
