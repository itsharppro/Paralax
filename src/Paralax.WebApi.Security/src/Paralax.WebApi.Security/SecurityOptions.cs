using System.Collections.Generic;

namespace Paralax.WebApi.Security
{
    public class SecurityOptions
    {
        public CertificateOptions Certificate { get; set; }

        public class CertificateOptions
        {
            // Determines if certificate validation is enabled
            public bool Enabled { get; set; }

            // The header name to extract the certificate from
            public string Header { get; set; }

            // Allows subdomains for domain validation
            public bool AllowSubdomains { get; set; }

            // List of allowed domains for certificate validation
            public IEnumerable<string> AllowedDomains { get; set; }

            // List of allowed hosts for certificate validation
            public IEnumerable<string> AllowedHosts { get; set; }

            // Access control list for certificate validation
            public IDictionary<string, AclOptions> Acl { get; set; }

            // Skip checking the revocation status of the certificate
            public bool SkipRevocationCheck { get; set; }

            // Get the name of the header, defaulting to "Certificate" if not set
            public string GetHeaderName() => string.IsNullOrWhiteSpace(Header) ? "Certificate" : Header;

            // ACL options for certificate permissions
            public class AclOptions
            {
                // The valid issuer of the certificate
                public string ValidIssuer { get; set; }

                // The valid thumbprint of the certificate
                public string ValidThumbprint { get; set; }

                // The valid serial number of the certificate
                public string ValidSerialNumber { get; set; }

                // Permissions associated with the certificate
                public IEnumerable<string> Permissions { get; set; }
            }
        }
    }
}
