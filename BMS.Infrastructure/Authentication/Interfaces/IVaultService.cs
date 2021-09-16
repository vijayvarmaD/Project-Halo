using System;
using System.Threading.Tasks;

namespace BMS.Infrastructure.Authentication.Interfaces
{
    public interface IVaultService
    {
        Task<string> ReadPublicKey();
        Task<string> ReadPrivateKey();
    }
}
