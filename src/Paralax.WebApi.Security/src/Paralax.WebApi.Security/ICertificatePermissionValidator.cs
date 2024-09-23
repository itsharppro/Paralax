using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Paralax.WebApi.Security
{
    public interface ICertificatePermissionValidator
    {
        bool HasAccess(X509Certificate2 certificate, IEnumerable<string> permissions, HttpContext context);
    }
}