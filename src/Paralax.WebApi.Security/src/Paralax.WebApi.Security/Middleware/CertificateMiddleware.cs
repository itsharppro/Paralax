using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Paralax.WebApi.Security.Middleware
{
    internal sealed class CertificateMiddleware : IMiddleware
    {
        private readonly ICertificatePermissionValidator _certificatePermissionValidator;
        private readonly ILogger<CertificateMiddleware> _logger;
        private readonly SecurityOptions.CertificateOptions _options;
        private readonly HashSet<string> _allowedHosts;
        private readonly IDictionary<string, SecurityOptions.CertificateOptions.AclOptions> _acl;
        private readonly IDictionary<string, string> _subjects = new Dictionary<string, string>();
        private readonly bool _validateAcl;
        private readonly bool _skipRevocationCheck;

        public CertificateMiddleware(ICertificatePermissionValidator certificatePermissionValidator,
            SecurityOptions options, ILogger<CertificateMiddleware> logger)
        {
            _certificatePermissionValidator = certificatePermissionValidator;
            _logger = logger;
            _options = options.Certificate;
            _allowedHosts = new HashSet<string>(_options.AllowedHosts ?? Array.Empty<string>());
            _validateAcl = _options.Acl is not null && _options.Acl.Any();
            _skipRevocationCheck = options.Certificate.SkipRevocationCheck;

            if (!_validateAcl)
            {
                return;
            }

            _acl = new Dictionary<string, SecurityOptions.CertificateOptions.AclOptions>();
            foreach (var (key, acl) in _options.Acl)
            {
                if (!string.IsNullOrWhiteSpace(acl.ValidIssuer) && !acl.ValidIssuer.StartsWith("CN="))
                {
                    acl.ValidIssuer = $"CN={acl.ValidIssuer}";
                }

                var subject = key.StartsWith("CN=") ? key : $"CN={key}";
                if (_options.AllowSubdomains)
                {
                    foreach (var domain in options.Certificate.AllowedDomains ?? Enumerable.Empty<string>())
                    {
                        _subjects.Add($"{subject}.{domain}", key);
                    }
                }

                _acl.Add(_subjects.Any() ? key : subject, acl);
            }
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!_options.Enabled)
            {
                return next(context);
            }

            if (IsAllowedHost(context))
            {
                return next(context);
            }

            var certificate = context.Connection.ClientCertificate;
            if (certificate is null || !Verify(certificate))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            if (!_validateAcl)
            {
                return next(context);
            }

            if (!ValidateCertificateAcl(certificate, context))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            return next(context);
        }

        private bool Verify(X509Certificate2 certificate)
        {
            var chain = new X509Chain
            {
                ChainPolicy = new X509ChainPolicy()
                {
                    RevocationMode = _skipRevocationCheck ? X509RevocationMode.NoCheck : X509RevocationMode.Online,
                }
            };

            var chainBuilt = chain.Build(certificate);
            foreach (var chainElement in chain.ChainElements)
            {
                chainElement.Certificate.Dispose();
            }

            if (chainBuilt)
            {
                return true;
            }

            _logger.LogError("Certificate validation failed");
            foreach (var chainStatus in chain.ChainStatus)
            {
                _logger.LogError($"Chain error: {chainStatus.Status} -> {chainStatus.StatusInformation}");
            }

            return false;
        }

        private bool ValidateCertificateAcl(X509Certificate2 certificate, HttpContext context)
        {
            // Get the ACL options based on the certificate subject
            if (_subjects.TryGetValue(certificate.Subject, out var subject))
            {
                if (!_acl.TryGetValue(subject, out var acl))
                {
                    return false;
                }

                return ValidateAclOptions(acl, certificate, context);
            }

            if (!_acl.TryGetValue(certificate.Subject, out var defaultAcl))
            {
                return false;
            }

            return ValidateAclOptions(defaultAcl, certificate, context);
        }

        private bool ValidateAclOptions(SecurityOptions.CertificateOptions.AclOptions acl, X509Certificate2 certificate, HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(acl.ValidIssuer) && certificate.Issuer != acl.ValidIssuer)
            {
                _logger.LogError($"Certificate issuer {certificate.Issuer} does not match ACL issuer {acl.ValidIssuer}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(acl.ValidThumbprint) && certificate.Thumbprint != acl.ValidThumbprint)
            {
                _logger.LogError($"Certificate thumbprint {certificate.Thumbprint} does not match ACL thumbprint {acl.ValidThumbprint}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(acl.ValidSerialNumber) && certificate.SerialNumber != acl.ValidSerialNumber)
            {
                _logger.LogError($"Certificate serial number {certificate.SerialNumber} does not match ACL serial number {acl.ValidSerialNumber}");
                return false;
            }

            if (acl.Permissions is null || !acl.Permissions.Any())
            {
                return true;
            }

            return _certificatePermissionValidator.HasAccess(certificate, acl.Permissions, context);
        }

        private bool IsAllowedHost(HttpContext context)
        {
            var host = context.Request.Host.Host;
            if (_allowedHosts.Contains(host))
            {
                return true;
            }

            return context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor) &&
                   _allowedHosts.Contains(forwardedFor);
        }
    }
}
