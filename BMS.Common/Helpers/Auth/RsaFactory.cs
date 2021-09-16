using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.IO;
using System.Security.Cryptography;

namespace BMS.Common.Helpers.Auth
{
    public class RsaFactory
    {
        public RSACryptoServiceProvider ReadPrivateKey(string privateKey)
        {
            try
            {
                var filePath = "D:/Code Repository/Learn/BMS.API.Identity/keys/bms-private.pem";
                using (TextReader privateKeyTextReader = new StringReader(File.ReadAllText(filePath)))
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

        public RSACryptoServiceProvider ReadPublicKey(string publicKey)
        {
            try
            {
                var filePath = "D:/Code Repository/Learn/BMS.API.Identity/keys/bms-public.pem";
                using (TextReader publicKeyTextReader = new StringReader(File.ReadAllText(filePath)))
                {
                    RsaKeyParameters readKeyPair = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

                    //RsaPrivateCrtKeyParameters privateKeyParams = ((RsaPrivateCrtKeyParameters)readKeyPair.Public);
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

        public RSACryptoServiceProvider ReadPublicKeyVault(string publicKey)
        {
            try
            {
                using (TextReader publicKeyTextReader = new StringReader(publicKey))
                {
                    RsaKeyParameters readKeyPair = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

                    //RsaPrivateCrtKeyParameters privateKeyParams = ((RsaPrivateCrtKeyParameters)readKeyPair.Public);
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
    }
}
