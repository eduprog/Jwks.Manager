using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Jwks.Manager.Interfaces
{
    public interface IJsonWebKeySetService
    {
        SigningCredentials Generate(JwksOptions options = null);

        SigningCredentials GetCurrent(JwksOptions options = null);
        IReadOnlyCollection<JsonWebKey> GetLastKeysCredentials(int qty);
    }
}