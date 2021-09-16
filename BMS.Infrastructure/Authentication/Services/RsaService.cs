using BMS.Infrastructure.Authentication.Helpers;
using BMS.Infrastructure.Authentication.Interfaces;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BMS.Infrastructure.Authentication.Services
{
    public class RsaService : IRsaService
    {
        public RsaService(IConfiguration configuration)
        {

        }
        public RSACryptoServiceProvider ReadPublicKey(string publicKey)
        {
            try
            {
                using (TextReader publicKeyTextReader = new StringReader(publicKey))
                {
                    RsaKeyParameters readKeyPair = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();
                    RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                    RSAParameters parms = new RSAParameters();
                    parms.Modulus = readKeyPair.Modulus.ToByteArrayUnsigned();
                    parms.Exponent = readKeyPair.Exponent.ToByteArrayUnsigned();
                    cryptoServiceProvider.ImportParameters(parms);
                    return cryptoServiceProvider;
                }
            }
            catch (CryptographicException c)
            {
                throw c;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RSACryptoServiceProvider ReadPrivateKey(string privateKey)
        {
            try
            {
                using (TextReader privateKeyTextReader = new StringReader(privateKey))
                {
                    AsymmetricCipherKeyPair readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader, new PasswordFinder("1234")).ReadObject();
                    RsaPrivateCrtKeyParameters privateKeyParams = ((RsaPrivateCrtKeyParameters)readKeyPair.Private);
                    RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                    RSAParameters parms = new RSAParameters();
                    parms.Modulus = privateKeyParams.Modulus.ToByteArrayUnsigned();
                    parms.P = privateKeyParams.P.ToByteArrayUnsigned();
                    parms.Q = privateKeyParams.Q.ToByteArrayUnsigned();
                    parms.DP = privateKeyParams.DP.ToByteArrayUnsigned();
                    parms.DQ = privateKeyParams.DQ.ToByteArrayUnsigned();
                    parms.InverseQ = privateKeyParams.QInv.ToByteArrayUnsigned();
                    parms.D = privateKeyParams.Exponent.ToByteArrayUnsigned();
                    parms.Exponent = privateKeyParams.PublicExponent.ToByteArrayUnsigned();
                    cryptoServiceProvider.ImportParameters(parms);
                    return cryptoServiceProvider;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
