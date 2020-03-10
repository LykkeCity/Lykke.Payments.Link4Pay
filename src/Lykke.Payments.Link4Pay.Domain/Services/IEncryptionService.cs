using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Lykke.Payments.Link4Pay.Domain.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string data);
        string Decrypt(string data);
        Dictionary<string, string> GetHeaders(string apiType, string transactionId);
        Task InitAsync(string vaultBaseUrl, string certificateName, string passwordKey);
        X509Certificate2 GetCertificate();
    }
}
