using System;
using System.Collections.Generic;

namespace Paralax.WebApi
{
    public class WebApiEndpointDefinitions : List<WebApiEndpointDefinition>
    {
    }

    public class WebApiEndpointDefinition
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public IEnumerable<WebApiEndpointParameter> Parameters { get; set; } = new List<WebApiEndpointParameter>();
        public IEnumerable<WebApiEndpointResponse> Responses { get; set; } = new List<WebApiEndpointResponse>();
        
        public bool RequiresAuth { get; set; } = false;
        public string[] Roles { get; set; } = Array.Empty<string>(); 
        public string[] Policies { get; set; } = Array.Empty<string>(); 
    }

    public class WebApiEndpointParameter
    {
        public string In { get; set; } // Location of the parameter (query, path, body, etc.)
        public Type Type { get; set; } // Type of the parameter (e.g., int, string, etc.)
        public string Name { get; set; } // Parameter name
        public bool Required { get; set; } = true; // Whether this parameter is required
        public object Example { get; set; } // Example value for the parameter (used in documentation)
    }

    public class WebApiEndpointResponse
    {
        public Type Type { get; set; } // Type of the response (e.g., UserResponse, ErrorResponse, etc.)
        public int StatusCode { get; set; } // HTTP status code for this response
        public object Example { get; set; } // Example response content (used in documentation)
    }
}
