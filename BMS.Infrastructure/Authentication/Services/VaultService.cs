using VaultSharp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using VaultSharp.V1.Commons;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using System;
using BMS.Infrastructure.Authentication.Interfaces;

namespace BMS.Infrastructure.Authentication.Services
{
    public class VaultService : IVaultService
    {
        private IVaultClient _vaultClient;

        public VaultService(IConfiguration configuration)
        {
            string vaultToken = string.Empty;
            if ((configuration["AppName"] == "BMS.API.Gateway") || (configuration["AppName"] == "BMS.API.Movies"))
            {
                vaultToken = configuration["Vault:Token"];
            }
            else if ((configuration["AppName"] == "BMS.API.Identity"))
            {
                vaultToken = configuration["Vault:Token"];
            }
            else
            {
                vaultToken = "";
            }

            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);

            var vaultClientSettings = new VaultClientSettings("http://127.0.0.1:8200/", authMethod);
            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<string> ReadPublicKey()
        {
            try
            {
                var secret = await ReadSecret("public", null);
                return (string)secret.Data.Data["rsakey"];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> ReadPrivateKey()
        {
            try
            {
                var secret = await ReadSecret("private", null);
                return (string)secret.Data.Data["rsakey"];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> ReadPrivateKeyPassword()
        {
            try
            {
                var secret = await ReadSecret("private", null);
                return (string)secret.Data.Data["password"];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<Secret<SecretData>> ReadSecret(string path, int? version = null)
        {
            try
            {
                Secret<SecretData> kv2Secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, version, "bms");
                return kv2Secret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
