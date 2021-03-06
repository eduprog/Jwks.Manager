using Jwks.Manager.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jwks.Manager.Store.FileSystem
{
    public class FileSystemStore : IJsonWebKeyStore
    {
        private readonly IOptions<JwksOptions> _options;
        public DirectoryInfo KeysPath { get; }

        public FileSystemStore(DirectoryInfo keysPath, IOptions<JwksOptions> options)
        {
            _options = options;
            KeysPath = keysPath;
        }

        private string GetCurrentFile()
        {
            return Path.Combine(KeysPath.FullName, $"{_options.Value.KeyPrefix}current.key");
        }

        public void Save(SecurityKeyWithPrivate securityParamteres)
        {
            if (!KeysPath.Exists)
                KeysPath.Create();

            // Datetime it's just to be easy searchable.
            if (File.Exists(GetCurrentFile()))
                File.Copy(GetCurrentFile(), Path.Combine(Path.GetDirectoryName(GetCurrentFile()), $"{_options.Value.KeyPrefix}old-{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid()}.key"));

            File.WriteAllText(GetCurrentFile(), JsonConvert.SerializeObject(securityParamteres));
        }

        public bool NeedsUpdate()
        {
            return !File.Exists(GetCurrentFile()) || File.GetCreationTimeUtc(GetCurrentFile()).AddDays(_options.Value.DaysUntilExpire) < DateTime.UtcNow.Date;
        }

        public void Update(SecurityKeyWithPrivate securityKeyWithPrivate)
        {
            foreach (var fileInfo in KeysPath.GetFiles("*.key"))
            {
                var key = GetKey(fileInfo.FullName);
                if (key.Id != securityKeyWithPrivate.Id) continue;

                File.WriteAllText(fileInfo.FullName, JsonConvert.SerializeObject(securityKeyWithPrivate));
                break;
            }
        }


        public SecurityKeyWithPrivate GetCurrentKey()
        {
            return GetKey(GetCurrentFile());
        }

        private SecurityKeyWithPrivate GetKey(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException("Check configuration - cannot find auth key file: " + file);
            var keyParams = JsonConvert.DeserializeObject<SecurityKeyWithPrivate>(File.ReadAllText(file));
            return keyParams;

        }

        public IReadOnlyCollection<SecurityKeyWithPrivate> Get(int quantity = 5)
        {
            return
                KeysPath.GetFiles("*.key")
                    .Take(quantity)
                    .Select(s => s.FullName)
                    .Select(GetKey).ToList().AsReadOnly();
        }

        public void Clear()
        {
            if (KeysPath.Exists)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                foreach (var fileInfo in KeysPath.GetFiles($"*.key"))
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
