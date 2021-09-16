using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BMS.Infrastructure.Authentication.Interfaces
{
    public interface IRsaService
    {
        RSACryptoServiceProvider ReadPublicKey(string publicKey);
        RSACryptoServiceProvider ReadPrivateKey(string privateKey);
    }
}
