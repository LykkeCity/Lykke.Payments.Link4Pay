using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.DomainServices
{
    public class Link4PayApiService : ILink4PayApiService
    {
        private readonly Link4PaySettings _link4PaySettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ILog _log;

        public Link4PayApiService(
            Link4PaySettings link4PaySettings,
            IEncryptionService encryptionService,
            ILogFactory logFactory
            )
        {
            _link4PaySettings = link4PaySettings;
            _encryptionService = encryptionService;
            _log = logFactory.CreateLog(this);
        }

        public async Task<string> GetPaymentUrlAsync(CardPaymentRequest request)
        {
            CardPaymentResponse result = await SendRequestAsync<CardPaymentResponse>(request, Link4PayApiType.ClientAuth, request.Transaction.TxnReference);

            if (result == null)
                return string.Empty;

            string url = _encryptionService.Decrypt(result.Payload);
            string gatewayReference = url.Replace("//", "/").Split('/')[2];
            string data = GetRedirectData(gatewayReference, request.Transaction.TxnReference);

            return $"{_link4PaySettings.ExternalUrl}/redirect?payload={data}&url={url.ToBase64()}";
        }

        public async Task CreateWebHookAsync(string url, List<string> events)
        {
            var data = await SendRequestAsync<object>(
                new {webhook = new {events = events, url = url}, merchantID = _link4PaySettings.ClientId},
                Link4PayApiType.CreateWebhook);
        }

        public Task<TransactionStatusResponse> GetTransactionInfoAsync(string transactionId)
        {
            return SendRequestAsync<TransactionStatusResponse>(
                new {merchantID = _link4PaySettings.ClientId, txnReference = transactionId}, Link4PayApiType.TransactionStatus);
        }

        private async Task<T> SendRequestAsync<T>(object payload, string apiType, string transactionId = null)
        {
            string json = JsonConvert.SerializeObject(payload, new BooleanJsonConverter());
            string encryptedPayload = _encryptionService.Encrypt(json);

            string data = new {apiKey = _link4PaySettings.ClientId, payLoad = encryptedPayload}.ToJson();
            var headers = _encryptionService.GetHeaders(apiType, transactionId);
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(_link4PaySettings.PaymentUrl);
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.KeepAlive = false;

            foreach (var item in headers)
            {
                httpWebRequest.Headers.Add(item.Key, item.Value);
            }
            httpWebRequest.ClientCertificates.Add(_encryptionService.GetCertificate());
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = data.Length;

            string result = string.Empty;

            try
            {
                using (var requestStream = await httpWebRequest.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
                }

                using (var response = (HttpWebResponse) await httpWebRequest.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                string errorContext = null;

                if (ex.Response != null)
                {
                    using (var response = (HttpWebResponse) ex.Response)
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                errorContext = streamReader.ReadToEnd();
                            }
                        }
                    }
                }

                _log.Error(ex, ex.Message, errorContext);

                return default(T);
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.Message);
            }

            return string.IsNullOrEmpty(result)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(result);
        }

        private string GetRedirectData(string gatewayReference, string transactionId)
        {
            var headers = _encryptionService.GetHeaders(Link4PayApiType.WithHpp, transactionId);

            return _encryptionService.Encrypt(new
            {
                Authorization = headers[Link4PayConsts.Headers.Authorization],
                dateHeader = headers[Link4PayConsts.Headers.Date],
                gatewayReference = gatewayReference,
                merchantID = _link4PaySettings.ClientId
            }.ToJson());
        }
    }
}
