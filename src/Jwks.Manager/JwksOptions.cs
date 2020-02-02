using Microsoft.IdentityModel.Tokens;

namespace Jwks.Manager
{
    public class JwksOptions
    {
        public KeyFormat Format { get; set; } = KeyFormat.RSA;
        public string Algorithm { get; set; } = SecurityAlgorithms.RsaSsaPssSha256;
        public int DaysUntilExpire { get; set; } = 90;
    }
}