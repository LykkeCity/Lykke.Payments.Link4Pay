using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Microsoft.Azure.KeyVault;

namespace Lykke.Payments.Link4Pay.DomainServices
{
    public class EncryptionService : IEncryptionService
    {
        private readonly KeyVaultClient _keyVaultClient;
        private readonly string _accessToken;
        private readonly string _clientId;
        private X509Certificate2 _certificate;
        private string _thumbPrint;
        private byte[] _password;
        private byte[] _decryptKey;

        public EncryptionService(
            KeyVaultClient keyVaultClient,
            Link4PaySettings link4PaySettings,
            KeyVaultSettings keyVaultSettings
        )
        {
            _keyVaultClient = keyVaultClient;
            _accessToken = link4PaySettings.AccessToken;
            _clientId = link4PaySettings.ClientId;
        }

        public string Encrypt(string data)
        {
            Random random = new Random();
            var iv = new byte[16];
            random.NextBytes(iv);
            var bufferString = iv.ToHexString();

            var rijndael = new RijndaelManaged
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Key = _password,
                IV = iv
            };

            string resultData = string.Empty;

            using (var encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
                streamWriter.Close();
                cryptoStream.Close();
                rijndael.Clear();
                resultData = memoryStream.ToArray().ToHexString();
            }

            return $"{bufferString.ToLower()}{resultData.ToLower()}";
        }

        public string Decrypt(string data)
        {
            byte[] iv = Utils.HexToArray(data.Substring(0, 32));
            byte[] encruptedData = Utils.HexToArray(data.Substring(32));

            var rijndaelManaged = new RijndaelManaged
            {
                Padding = PaddingMode.None,
                Mode = CipherMode.CBC,
                Key = _decryptKey,
                IV = iv
            };

            using (var decryptor = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV))
            using (var memoryStream = new MemoryStream(encruptedData))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var streamReader = new StreamReader(cryptoStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public Dictionary<string, string> GetHeaders(string apiType, string transactionId)
        {
            var utcNow = DateTime.UtcNow;
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Calcutta");
            string date = TimeZoneInfo
                .ConvertTimeFromUtc(utcNow, timezone)
                .ToString("HHmmssddMMyyyy");

            var hmac = HMAC.Create("HMACSHA256");
            hmac.Key = Encoding.UTF8.GetBytes(_accessToken);

            string hashString = $"{apiType}+/pArTnErApI+{date}+{transactionId}";
            string base64String = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(hashString)));

            string authHeader = $"hmac {_clientId}:{transactionId}:{base64String}";
            string dateHeader = utcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";

            return new Dictionary<string, string>
            {
                {Link4PayConsts.Headers.Authorization, authHeader},
                {Link4PayConsts.Headers.Date, dateHeader}
            };
        }

        public X509Certificate2 GetCertificate()
        {
            return _certificate;
        }

        public async Task InitAsync(string valultUrl, string certName, string passwordKey)
        {
            var certDataTask = _keyVaultClient.GetSecretAsync(valultUrl, certName);
            var passwordTask = _keyVaultClient.GetSecretAsync(valultUrl, passwordKey);

            await Task.WhenAll(certDataTask, passwordTask);
            var cert = Convert.FromBase64String(certDataTask.Result.Value);
            _certificate = new X509Certificate2(cert, passwordTask.Result.Value);
            _thumbPrint = Regex.Replace(_certificate.Thumbprint, ".{2}", "$0:").TrimEnd(':');
            _password = GeneratePassword(_accessToken, _thumbPrint);
            _decryptKey = Encoding.ASCII.GetBytes(_thumbPrint.Substring(0, 32));
        }

        private byte[] GeneratePassword(string accessToken, string thumbPrint)
        {
            byte[] bytes1 = Encoding.ASCII.GetBytes(thumbPrint);
            byte[] bytes2 = Encoding.ASCII.GetBytes(accessToken);
            int length = bytes1.Length;
            string password = string.Empty;
            int index = 0;

            while (index < accessToken.Length)
            {
                password += (char) (bytes2[index] | bytes1[index % length]);
                ++index;
            }

            return Encoding.UTF8.GetBytes(password);
        }
    }
}
