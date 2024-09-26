using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Paralax.Secrets.Vault.Core
{
    public class EmptyCertificatesIssuer : ICertificatesIssuer
    {
        public Task<X509Certificate2> IssueAsync()
        {
            return Task.FromResult<X509Certificate2>(null);
        }
    }
}