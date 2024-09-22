using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public class HttpResult<T>
    {
        public T Result { get; }
        public HttpResponseMessage Response { get; }
        public bool HasResult => Result is not null;
        public bool IsSuccess => Response?.IsSuccessStatusCode == true;
        public string ErrorMessage { get; }

        public HttpResult(T result, HttpResponseMessage response, string errorMessage = null)
        {
            Result = result;
            Response = response;
            ErrorMessage = errorMessage;
        }
    }
}