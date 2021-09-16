using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace BMS.Common.Helpers.Auth
{
    public class VaultFactory
    {
        private IVaultClient _vaultClient;

        public VaultFactory()
        {
            // remove
            string vaultToken = "s.1fUreYh0CjsTwdYUqiFmA6jn";

            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings("http://127.0.0.1:8200/", authMethod);
            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<string> ReadPublicKey()
        {
            Secret<SecretData> kv2Secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("public", null, "bms");
            return (string)kv2Secret.Data.Data["rsakey"];
        }
    }
}
