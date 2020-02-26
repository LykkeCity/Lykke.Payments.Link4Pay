using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Grpc.Core;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Lykke.Service.FeeCalculator.Client;

namespace Lykke.Payments.Link4Pay.Services
{
    public class Link4PayServiceImpl : Link4PayService.Link4PayServiceBase
    {
        private readonly Link4PaySettings _link4PaySettings;
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private readonly IRawLogRepository _rawLogRepository;
        private readonly ILink4PayApiService _link4PayApiService;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly IReadOnlyList<string> _supportedCountries;
        private readonly IReadOnlyList<string> _supportedCurrencies;
        private readonly ILog _log;
        private string _successUrl;
        private string _failUrl;
        private string _cancelUrl;


        public Link4PayServiceImpl(
            Link4PaySettings link4PaySettings,
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPaymentTransactionEventsLog paymentTransactionEventsLog,
            IRawLogRepository rawLogRepository,
            ILink4PayApiService link4PayApiService,
            IFeeCalculatorClient feeCalculatorClient,
            IReadOnlyList<string> supportedCountries,
            IReadOnlyList<string> supportedCurrencies,
            ILogFactory logFactory
            )
        {
            _link4PaySettings = link4PaySettings;
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _rawLogRepository = rawLogRepository;
            _link4PayApiService = link4PayApiService;
            _feeCalculatorClient = feeCalculatorClient;
            _supportedCountries = supportedCountries;
            _supportedCurrencies = supportedCurrencies;
            _log = logFactory.CreateLog(this);
            _successUrl = $"{_link4PaySettings.ExternalUrl}/ok";
            _failUrl = $"{_link4PaySettings.ExternalUrl}/fail";
            _cancelUrl = $"{_link4PaySettings.ExternalUrl}/cancel";
        }

        public override async Task<PaymentUrlResponse> GetPaymentUrl(PaymentUrlRequest request, ServerCallContext context)
        {
            try
            {
                string errorMessage = string.Empty;
                ErrorDetails.Types.ErrorType? errorType = null;

                if (_supportedCurrencies.Any() && !_supportedCurrencies.Contains(request.Transaction.AssetId))
                {
                    errorMessage = $"Asset {request.Transaction.AssetId} is not supported";
                    errorType = ErrorDetails.Types.ErrorType.CurrencyNotSupported;
                }

                if (_supportedCountries.Any() && !string.IsNullOrEmpty(request.Details.CountryIso3) && !_supportedCountries.Contains(request.Details.CountryIso3))
                {
                    errorMessage = $"Country {request.Details.CountryIso3} is not supported";
                    errorType = ErrorDetails.Types.ErrorType.CountryNotSupported;
                }

                if (errorType != null)
                {
                    _log.Warning(errorMessage);

                    return new PaymentUrlResponse
                    {
                        Error = new ErrorDetails
                        {
                            ErrorType = errorType.Value,
                            Message = errorMessage
                        }
                    };
                }

                var bankCardsFees = await _feeCalculatorClient.GetBankCardFees();
                var feeAmount = Math.Round(request.Transaction.Amount * bankCardsFees.Percentage, 15);
                var totalAmount = (decimal) request.Transaction.Amount + (decimal) feeAmount;

                var url = await _link4PayApiService.GetPaymentUrlAsync(new CardPaymentRequest
                {
                    Merchant = new MerchantInfo {CustomerId = request.Transaction.ExternalClientId, MerchantId = _link4PaySettings.ClientId},
                    Transaction =
                        new TransactionInfo
                        {
                            TxnAmount = totalAmount.ToString("F2"),
                            CurrencyCode = request.Transaction.AssetId,
                            TxnReference = request.Transaction.TransactionId,
                            Payout = false
                        },
                    Customer = new CustomerInfo
                    {
                        BillingAddress = new AddressInfo
                        {
                            FirstName = request.Details?.FirstName,
                            LastName = request.Details?.LastName,
                            EmailId = request.Details?.Email,
                            MobileNo = request.Details?.Phone
                        }
                    },
                    Url = new UrlInfo
                    {
                        SuccessUrl = $"{(string.IsNullOrEmpty(request.Urls?.OkUrl?.Trim()) ? _successUrl : request.Urls?.OkUrl)}?transactionId={request.Transaction.TransactionId}",
                        FailUrl =  string.IsNullOrEmpty(request.Urls?.FailUrl?.Trim())
                            ? _failUrl
                            : request.Urls?.FailUrl,
                        CancelUrl =  string.IsNullOrEmpty(request.Urls?.CancelUrl?.Trim())
                            ? _cancelUrl
                            : request.Urls?.CancelUrl
                    }
                });

                if (string.IsNullOrEmpty(url))
                {
                    return new PaymentUrlResponse
                    {
                        Error = new ErrorDetails
                        {
                            ErrorType = ErrorDetails.Types.ErrorType.Unknown,
                            Message = "Error getting the payment url"
                        }
                    };
                }

                await _rawLogRepository.RegisterEventAsync(
                    RawLogEvent.Create("Payment Url has been created", request.ToJson()),
                    request.Transaction.ClientId);

                var info = OtherPaymentInfo.Create(
                        firstName: request.Details.FirstName,
                        lastName: request.Details.LastName,
                        city: string.Empty,
                        zip: string.Empty,
                        address: string.Empty,
                        country: request.Details.CountryIso3,
                        email: request.Details.Email,
                        contactPhone: request.Details.Phone,
                        dateOfBirth: string.Empty)
                    .ToJson();

                var pt = PaymentTransaction.Create(
                    request.Transaction.TransactionId,
                    CashInPaymentSystem.Link4Pay,
                    request.Transaction.ClientId,
                    request.Transaction.Amount,
                    feeAmount,
                    request.Transaction.AssetId,
                    null,
                    request.Transaction.AssetId,
                    info
                    );

                await Task.WhenAll(
                    _paymentTransactionsRepository.CreateAsync(pt),
                    _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(request.Transaction.TransactionId, "",
                        "Registered", request.Transaction.ClientId)),
                    _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(request.Transaction.TransactionId, url,
                        "Payment Url has created", request.Transaction.ClientId))
                );

                return new PaymentUrlResponse
                {
                    PaymentUrl = url,
                    OkUrl = $"{(string.IsNullOrEmpty(request.Urls?.OkUrl?.Trim()) ? _successUrl : request.Urls?.OkUrl)}?transactionId={request.Transaction.TransactionId}",
                    FailUrl =  string.IsNullOrEmpty(request.Urls?.FailUrl?.Trim())
                        ? _failUrl
                        : request.Urls?.FailUrl,
                    CancelUrl =  string.IsNullOrEmpty(request.Urls?.CancelUrl?.Trim())
                        ? _cancelUrl
                        : request.Urls?.CancelUrl
                };
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                return new PaymentUrlResponse
                {
                    Error = new ErrorDetails
                    {
                        ErrorType = ErrorDetails.Types.ErrorType.Unknown,
                        Message = "Error getting the payment url"
                    }
                };
            }
        }

        public override async Task<TransactionInfoResponse> GetTransactionInfo(TransactionRequest request, ServerCallContext context)
        {
            var transaction = await  _paymentTransactionsRepository.GetByTransactionIdAsync(request.TransactionId);

            return new TransactionInfoResponse
            {
                AssetId = transaction?.AssetId ?? string.Empty,
                Amount = transaction?.Amount ?? 0
            };
        }
    }
}
