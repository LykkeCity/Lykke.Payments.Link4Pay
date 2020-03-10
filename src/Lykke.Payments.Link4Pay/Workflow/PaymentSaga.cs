using System.Linq;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Payments.Link4Pay.Contract;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Workflow.Commands;
using Lykke.Payments.Link4Pay.Workflow.Events;

namespace Lykke.Payments.Link4Pay.Workflow
{
    public class PaymentSaga
    {
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentNotifier[] _paymentNotifiers;
        private readonly string _sourceClientId;

        public PaymentSaga(
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPaymentNotifier[] paymentNotifiers,
            string sourceClientId
            )
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentNotifiers = paymentNotifiers;
            _sourceClientId = sourceClientId;
        }

        public async Task Handle(ProcessingStartedEvent evt, ICommandSender commandSender)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(evt.OrderId);

            var transferCommand = new CreateTransferCommand
            {
                OrderId = evt.OrderId,
                TransferId = transaction.MeTransactionId,
                AssetId = transaction.AssetId,
                ClientId = transaction.ClientId,
                SourceClientId = _sourceClientId,
                Amount = transaction.Amount,
                WalletId = transaction.WalletId
            };

            commandSender.SendCommand(transferCommand, "me");
        }

        public Task Handle(TransferCreatedEvent evt, ICommandSender commandSender)
        {
            var command = new CompleteTransferCommand { Id = evt.OrderId, Amount = evt.Amount };

            commandSender.SendCommand(command, Link4PayBoundedContext.Name);

            return Task.CompletedTask;
        }

        public async Task Handle(TransferCompletedEvent evt, ICommandSender commandSender)
        {
            var transaction = await _paymentTransactionsRepository.GetByTransactionIdAsync(evt.OrderId);

            var tasks = _paymentNotifiers
                .Select(x => x.NotifyAsync(transaction))
                .ToList();

            await Task.WhenAll(tasks);
        }
    }
}
