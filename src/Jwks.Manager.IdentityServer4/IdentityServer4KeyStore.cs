using IdentityServer4.Models;
using IdentityServer4.Stores;
using Jwks.Manager.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Jwks.Manager.IdentityServer4
{
    internal class IdentityServer4KeyStore : IValidationKeysStore, ISigningCredentialStore
    {
        private readonly IJsonWebKeySetService _keyService;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<JwksOptions> _options;

        /// <summary>Constructor for IdentityServer4KeyStore.</summary>
        /// <param name="keyService"></param>
        /// <param name="memoryCache"></param>
        /// <param name="options"></param>
        public IdentityServer4KeyStore(IJsonWebKeySetService keyService, IMemoryCache memoryCache, IOptions<JwksOptions> options)
        {
            this._keyService = keyService;
            _memoryCache = memoryCache;
            _options = options;
        }

        /// <summary>Returns the current signing key.</summary>
        /// <returns></returns>
        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            if (!_memoryCache.TryGetValue("ASPNET-IS4-CURRENT-KEY", out SigningCredentials credentials))
            {
                credentials = _keyService.GetCurrent();
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                if (credentials != null)
                    _memoryCache.Set("ASPNET-IS4-CURRENT-KEY", credentials, cacheEntryOptions);
            }

            return Task.FromResult(credentials);
        }

        /// <summary>Returns all the validation keys.</summary>
        /// <returns></returns>
        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            if (!_memoryCache.TryGetValue("ASPNET-IS4-VALIDATION-KEY", out IReadOnlyCollection<JsonWebKey> credentials))
            {
                _keyService.GetCurrent();
                credentials = _keyService.GetLastKeysCredentials(_options.Value.AlgorithmsToKeep);

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                _memoryCache.Set("ASPNET-IS4-VALIDATION-KEY", credentials, cacheEntryOptions);
                _memoryCache.Remove("ASPNET-IS4-CURRENT-KEY");
            }
            return Task.FromResult(credentials.Select(s => new SecurityKeyInfo()
            {
                Key = s,
                SigningAlgorithm = s.Alg
            }));
        }
    }
}