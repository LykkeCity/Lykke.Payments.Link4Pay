using System;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog;
using Lykke.Payments.Link4Pay.Workflow.Commands;
using Lykke.Payments.Link4Pay.Workflow.Events;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.FeeCalculator.Client;

namespace Lykke.Payments.Link4Pay.Workflow
{
    public class MeCommandHandler
    {
        private readonly IExchangeOperationsServiceClient _exchangeOperationsService;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private readonly IFeeCalculatorClient _feeCalculatorClient;
        private readonly string _bankCardFeeClientId;
        private readonly AntiFraudChecker _antiFraudChecker;

        public MeCommandHandler(
            IExchangeOperationsServiceClient exchangeOperationsService,
            IPaymentTransactionEventsLog paymentTransactionEventsLog,
            IFeeCalculatorClient feeCalculatorClient,
            string bankCardFeeClientId,
            AntiFraudChecker antiFraudChecker)
        {
            _exchangeOperationsService = exchangeOperationsService;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _antiFraudChecker = antiFraudChecker;
            _feeCalculatorClient = feeCalculatorClient;
            _bankCardFeeClientId = bankCardFeeClientId;
        }

        public async Task<CommandHandlingResult> Handle(CreateTransferCommand createTransferCommand, IEventPublisher eventPublisher)
        {
            if (await _antiFraudChecker.IsPaymentSuspicious(createTransferCommand.ClientId, createTransferCommand.OrderId))
            {
                return new CommandHandlingResult { Retry = true, RetryDelay = (long)TimeSpan.FromMinutes(10).TotalMilliseconds };
            }

            var bankCardFees = await _feeCalculatorClient.GetBankCardFees();

            var result = await _exchangeOperationsService.TransferWithNotificationAsync(
                transferId: createTransferCommand.TransferId,
                destClientId: createTransferCommand.ClientId,
                sourceClientId: createTransferCommand.SourceClientId,
                amount: createTransferCommand.Amount,
                assetId: createTransferCommand.AssetId,
                feeClientId: _bankCardFeeClientId,
                feeSizePercentage: bankCardFees.Percentage,
                destWalletId: createTransferCommand.WalletId);

            if (!result.IsOk())
            {
                await _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(createTransferCommand.OrderId, "N/A", $"{result.Code}:{result.Message}", nameof(CreateTransferCommand)));
                return CommandHandlingResult.Ok();
            }

            eventPublisher.PublishEvent(new TransferCreatedEvent
            {
                OrderId = createTransferCommand.OrderId,
                TransferId = createTransferCommand.TransferId,
                ClientId = createTransferCommand.ClientId,
                Amount = createTransferCommand.Amount,
                AssetId = createTransferCommand.AssetId
            });

            return CommandHandlingResult.Ok();
        }
    }
}
