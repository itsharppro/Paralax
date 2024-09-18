using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using Paralax.Security.Core;
using System.Security.Cryptography;

namespace Paralax.Tests.Security
{
    public class SignerTests
    {
        private readonly Signer _signer;

        public SignerTests()
        {
            _signer = new Signer();
        }

        [Fact]
        public void Sign_WithValidData_ShouldReturnSignature()
        {
            // Arrange
            var data = new { Name = "Test" };
            var certificate = GetTestCertificate();

            // Act
            var result = _signer.Sign(data, certificate);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Verify_WithValidSignature_ShouldReturnTrue()
        {
            // Arrange
            var data = new { Name = "Test" };
            var certificate = GetTestCertificate();
            var signature = _signer.Sign(data, certificate);

            // Act
            var result = _signer.Verify(data, certificate, signature);

            // Assert
            Assert.True(result);
        }

        private X509Certificate2 GetTestCertificate()
        {
            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    "CN=TestCertificate",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                var certificate = request.CreateSelfSigned(
                    DateTimeOffset.Now.AddDays(-1), 
                    DateTimeOffset.Now.AddDays(365));

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }

    }
}
