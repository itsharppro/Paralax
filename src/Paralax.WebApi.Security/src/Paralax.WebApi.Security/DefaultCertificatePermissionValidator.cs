using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace Paralax.WebApi.Security
{
    internal sealed class DefaultCertificatePermissionValidator : ICertificatePermissionValidator
    {
        public bool HasAccess(X509Certificate2 certificate, 
            IEnumerable<string> permissions, HttpContext context)
        {
            return true;
        }
    }
}
