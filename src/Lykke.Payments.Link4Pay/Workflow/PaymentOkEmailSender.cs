using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Messages.Email;
using Lykke.Messages.Email.MessageData;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.PersonalData.Contract;

namespace Lykke.Payments.Link4Pay.Workflow
{
    public class PaymentOkEmailSender : IPaymentNotifier
    {
        private readonly IEmailSender _emailSender;
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ILog _log;

        public PaymentOkEmailSender(
            IEmailSender emailSender,
            IPersonalDataService personalDataService,
            IClientAccountClient clientAccountClient,
            ILogFactory logFactory)
        {
            _emailSender = emailSender;
            _personalDataService = personalDataService;
            _log = logFactory.CreateLog(this);
            _clientAccountClient = clientAccountClient;
        }

        public async Task NotifyAsync(IPaymentTransaction pt)
        {
            string emailAddress = string.Empty;

            try
            {
                var pdTask = _personalDataService.GetAsync(pt.ClientId);
                var accTask = _clientAccountClient.ClientAccountInformation.GetByIdAsync(pt.ClientId);

                await Task.WhenAll(pdTask, accTask);

                var acc = accTask.Result;
                var pd = pdTask.Result;

                emailAddress = pd.Email;

                await _emailSender.SendEmailAsync(acc.PartnerId, emailAddress, new DirectTransferCompletedData
                {
                    Amount = pt.Amount,
                    AssetId = pt.AssetId,
                    ClientName = pd.FullName
                });

                var body = $"Client: {pd.Email}, "
                    + $"Payment system amount: {pt.AssetId} {pt.Amount:0.00}, "
                    + $"Deposited amount: {pt.DepositedAssetId} {pt.DepositedAmount}, "
                    + $"PaymentSystem: {pt.PaymentSystem}";

                // email to Payments group
                await _emailSender.BroadcastEmailAsync(acc.PartnerId, BroadcastGroup.Payments, $"{pt.PaymentSystem}, payment notification Ok", body);
            }
            catch (Exception exc)
            {
                _log.Error("PaymentOkEmailSender.NotifyAsync", exc, emailAddress);
            }
        }
    }
}
