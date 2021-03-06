using Jwks.Manager.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace Jwks.Manager.Jwks
{
    /// <summary>
    /// Util class to allow restoring RSA/ECDsa parameters from JSON as the normal
    /// parameters class won't restore private key info.
    /// </summary>
    public class JwksService : IJsonWebKeySetService
    {
        private readonly IJsonWebKeyStore _store;
        private readonly IJsonWebKeyService _jwkService;
        private readonly IOptions<JwksOptions> _options;

        public JwksService(IJsonWebKeyStore store, IJsonWebKeyService jwkService, IOptions<JwksOptions> options)
        {
            _store = store;
            _jwkService = jwkService;
            _options = options;
        }

        public SigningCredentials Generate(JwksOptions options = null)
        {
            if (options == null)
                options = _options.Value;
            var key = _jwkService.Generate(options.Algorithm);
            var t = new SecurityKeyWithPrivate();
            t.SetParameters(key, options.Algorithm);
            _store.Save(t);
            return new SigningCredentials(key, options.Algorithm);
        }

        /// <summary>
        /// If current doesn't exist will generate new one
        /// </summary>
        public SigningCredentials GetCurrent(JwksOptions options = null)
        {
            if (_store.NeedsUpdate())
            {
                // According NIST - https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-57pt1r4.pdf - Private key should be removed when no longer needs
                RemovePrivateKeys();
                return Generate(options);
            }

            var currentKey = _store.GetCurrentKey();

            // options has change. Change current key
            if (!CheckCompatibility(currentKey, options))
                currentKey = _store.GetCurrentKey();

            return currentKey.GetSigningCredentials();
        }

        private void RemovePrivateKeys()
        {
            foreach (var securityKeyWithPrivate in _store.Get(_options.Value.AlgorithmsToKeep))
            {
                securityKeyWithPrivate.RemovePrivateKey();
                _store.Update(securityKeyWithPrivate);
            }
        }

        private bool CheckCompatibility(SecurityKeyWithPrivate currentKey, JwksOptions options)
        {
            if (options == null)
                options = _options.Value;

            if (currentKey.Algorithm == options.Algorithm) return true;

            Generate(options);
            return false;

        }

        public IReadOnlyCollection<JsonWebKey> GetLastKeysCredentials(int qty)
        {
            // sometimes current force update
            var store = _store.Get(qty);
            if (!store.Any())
            {
                GetCurrent();
                return _store.Get(qty).OrderByDescending(o => o.CreationDate).Select(s => s.GetSecurityKey()).ToList().AsReadOnly();
            }

            return store.OrderByDescending(o => o.CreationDate).Select(s => s.GetSecurityKey()).ToList().AsReadOnly();
        }
    }
}