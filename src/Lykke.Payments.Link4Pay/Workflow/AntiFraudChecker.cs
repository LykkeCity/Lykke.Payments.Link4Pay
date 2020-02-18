using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Messages.Email;
using Lykke.Messages.Email.MessageData;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Payments.Link4Pay.Workflow
{
    public class AntiFraudChecker
    {
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly DateTime _registrationDateSince;
        private readonly TimeSpan _paymentPeriod;

        private readonly IPersonalDataService _personalDataService;
        private readonly ICreditCardsService _creditCardsService;
        private readonly IEmailSender _emailSender;
        private readonly string _antifraudNotificationEmail;
        private readonly ILog _log;

        public AntiFraudChecker(
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IClientAccountClient clientAccountClient,
            DateTime registrationDateSince,
            TimeSpan paymentPeriod,
            IPersonalDataService personalDataService,
            ICreditCardsService creditCardsService,
            IEmailSender emailSender,
            string antifraudNotificationEmail,
            ILogFactory logFactory
            )
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _clientAccountClient = clientAccountClient;
            _registrationDateSince = registrationDateSince;
            _paymentPeriod = paymentPeriod;
            _personalDataService = personalDataService;
            _creditCardsService = creditCardsService;
            _emailSender = emailSender;
            _antifraudNotificationEmail = antifraudNotificationEmail;
            _log = logFactory.CreateLog(this);
        }

        public async Task<bool> IsPaymentSuspicious(string clientId, string orderId)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(orderId);
            if (transaction.AntiFraudStatus != null)
            {
                if (transaction.AntiFraudStatus == AntiFraudStatus.Pending.ToString())
                {
                    return true;
                }
                if (transaction.AntiFraudStatus == AntiFraudStatus.NotFraud.ToString())
                {
                    // FCT-? no extra information about the card
                    if (!string.IsNullOrWhiteSpace(transaction.CardHash))
                    {
                        await _creditCardsService.Approve(transaction.CardHash, clientId);
                    }

                    return false;
                }
            }

            var card = !string.IsNullOrEmpty(transaction.CardHash?.Trim())
                    ? await _creditCardsService.GetCard(transaction.CardHash, clientId)
                    : null;

            if (card == null)
                _log.Warning("Credit card is not found", context: clientId);
            else
            {
                if (card.Approved) // FCT-2
                    return false;
            }

            await _paymentTransactionsRepository.SetAntiFraudStatusAsync(orderId, AntiFraudStatus.Pending.ToString());
            await SendAntiFraudNotificationAsync(transaction);
            return true;
        }

        public async Task<bool> IsClientSuspicious(string clientId)
        {
            return await IsClientRegistrationFresh(clientId) || !await HasSuccessfulCreditCardDepositHistoryInPast(clientId);
        }

        private async Task<bool> IsClientRegistrationFresh(string clientId)
        {
            var registrationDate = (await _clientAccountClient.ClientAccountInformation.GetByIdAsync(clientId)).Registered;
            return registrationDate > _registrationDateSince;
        }

        private async Task<bool> HasSuccessfulCreditCardDepositHistoryInPast(string clientId)
        {
            return await _paymentTransactionsRepository.HasProcessedTransactionsAsync(clientId, DateTime.UtcNow.Subtract(_paymentPeriod));
        }

        private async Task<bool> IsCardUsedByOtherClients(string clientId, CreditCardModel card)
        {
            return (await _creditCardsService.GetCardsByHash(card.Hash)).Any(x => x.ClientId != clientId);
        }

        // private async Task<bool> IsCardHolderNameValid(string clientId, CreditCardModel card)
        // {
        //     var personalData = await _personalDataService.GetAsync(clientId);
        //     return IsNameSimilar(card.CardHolder, personalData.FirstName, personalData.LastName);
        // }

        // private static bool IsNameSimilar(string cardHolderName, string firstName, string lastName)
        // {
        //     return Compare(cardHolderName.ToLower(), $"{firstName} {lastName}".ToLowerInvariant())
        //            || Compare(cardHolderName.ToLower(), $"{lastName} {firstName}".ToLowerInvariant());
        // }

        // private static bool Compare(string expected, string actual)
        // {
        //     const double allowedSimilarity = 0.8;
        //     var metric = new SimMetrics.Net.Metric.Levenstein();
        //     var v = metric.GetSimilarity(expected, actual);
        //     return v > allowedSimilarity;
        // }

        private async Task SendAntiFraudNotificationAsync(IPaymentTransaction transaction)
        {
            if (string.IsNullOrEmpty(_antifraudNotificationEmail))
                return;

            var clientAccount = await _clientAccountClient.ClientAccountInformation.GetByIdAsync(transaction.ClientId);

            var msgData = new PlainTextData
            {
                Subject = "Review deposit request",
                Text = $"New deposit request (transactionId = {transaction.TransactionId}, clientId = {clientAccount.Id}, externalId = {clientAccount.ExternalId}), please review"
            };
            await _emailSender.SendEmailAsync(clientAccount.PartnerId, _antifraudNotificationEmail, msgData);
        }
    }

    public enum AntiFraudStatus
    {
        Pending,
        NotFraud
    }

    public enum AntiFraudSuspicionReason
    {
        None,
        InvalidCardHolderName,
        CardIsUsedByOtherClients
    }
}
