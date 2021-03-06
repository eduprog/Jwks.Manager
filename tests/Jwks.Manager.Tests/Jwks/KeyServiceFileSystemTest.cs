using FluentAssertions;
using Jwks.Manager.Interfaces;
using Jwks.Manager.Jwk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Jwks.Manager.Tests.Jwks
{
    public class KeyServiceFileSystemTest : IClassFixture<WarmupFileStore>
    {
        private readonly IJsonWebKeySetService _keyService;
        private readonly IJsonWebKeyStore _jsonWebKeyStore;
        public WarmupFileStore FileStoreWarmupData { get; }
        public KeyServiceFileSystemTest(WarmupFileStore fileStoreWarmup)
        {
            FileStoreWarmupData = fileStoreWarmup;
            _keyService = FileStoreWarmupData.Services.GetRequiredService<IJsonWebKeySetService>();
            _jsonWebKeyStore = FileStoreWarmupData.Services.GetRequiredService<IJsonWebKeyStore>();

        }

        [Fact]
        public void ShouldSaveCryptoInDatabase()
        {
            _keyService.GetCurrent();

            _keyService.GetLastKeysCredentials(5).Count.Should().BePositive();
        }

        [Theory]
        [InlineData(SecurityAlgorithms.HmacSha256, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.HmacSha384, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.HmacSha512, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.RsaSha256, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSha384, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSha512, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha256, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha384, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha512, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.EcdsaSha256, KeyType.ECDsa)]
        [InlineData(SecurityAlgorithms.EcdsaSha384, KeyType.ECDsa)]
        [InlineData(SecurityAlgorithms.EcdsaSha512, KeyType.ECDsa)]
        public void ShouldGenerate(string algorithm, KeyType keyType)
        {
            _keyService.Generate(new JwksOptions() { KeyPrefix = "ShouldGenerateManyRsa_", Algorithm = Algorithm.Create(algorithm, keyType) });
        }

        [Theory]
        [InlineData(SecurityAlgorithms.HmacSha256, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.HmacSha384, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.HmacSha512, KeyType.HMAC)]
        [InlineData(SecurityAlgorithms.RsaSha256, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSha384, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSha512, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha256, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha384, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.RsaSsaPssSha512, KeyType.RSA)]
        [InlineData(SecurityAlgorithms.EcdsaSha256, KeyType.ECDsa)]
        [InlineData(SecurityAlgorithms.EcdsaSha384, KeyType.ECDsa)]
        [InlineData(SecurityAlgorithms.EcdsaSha512, KeyType.ECDsa)]
        public void ShouldRemovePrivateAndUpdate(string algorithm, KeyType keyType)
        {
            var alg = Algorithm.Create(algorithm, keyType);
            var key = _keyService.Generate(new JwksOptions() { KeyPrefix = "ShouldGenerateManyRsa_", Algorithm = alg });
            var privateKey = new SecurityKeyWithPrivate();
            privateKey.SetParameters(key.Key, alg);
            _jsonWebKeyStore.Save(privateKey);

            /*Remove private*/
            privateKey.RemovePrivateKey();
            _jsonWebKeyStore.Update(privateKey);

        }
    }
}