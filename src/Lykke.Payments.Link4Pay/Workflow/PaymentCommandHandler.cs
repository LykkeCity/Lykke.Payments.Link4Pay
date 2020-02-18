﻿using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Cqrs;
using Lykke.Payments.Link4Pay.Contract.Events;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Workflow.Commands;
using Lykke.Payments.Link4Pay.Workflow.Events;

namespace Lykke.Payments.Link4Pay.Workflow
{
    public class PaymentCommandHandler
    {
        private readonly ILink4PayApiService _link4PayApiService;
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private ILog _log;

        public PaymentCommandHandler(
            ILink4PayApiService link4PayApiService,
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPaymentTransactionEventsLog paymentTransactionEventsLog,
            ILogFactory logFactory
            )
        {
            _link4PayApiService = link4PayApiService;
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _log = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashInCommand command, IEventPublisher eventPublisher)
        {
            var transactionStatus = await _link4PayApiService.GetTransactionInfoAsync(command.TransactionId);

            _log.Info("Transaction status", new {transactionId = transactionStatus.TxnReference, status = transactionStatus.Status.ToString()}.ToJson());

            if (transactionStatus.Status == TransactionStatus.Pending)
                return new CommandHandlingResult { Retry = true, RetryDelay = (long)TimeSpan.FromMinutes(1).TotalMilliseconds };

            var tx = await _paymentTransactionsRepository.GetByTransactionIdAsync(transactionStatus.TxnReference);

            if (tx != null && (tx.Status == PaymentStatus.NotifyDeclined || tx.Status == PaymentStatus.NotifyProcessed || tx.Status == PaymentStatus.Processing))
            {
                return CommandHandlingResult.Ok();
            }

            if (!string.IsNullOrEmpty(transactionStatus.Card?.CardHash))
            {
                await _paymentTransactionsRepository.SaveCardHashAsync(command.TransactionId, transactionStatus.Card.CardHash);

                if (tx != null)
                {
                    var evt = new CreditCardUsedEvent
                    {
                        ClientId = tx.ClientId,
                        OrderId = command.TransactionId,
                        CardHash = transactionStatus.Card.CardHash,
                        CardNumber = transactionStatus.Card.CardNo,
                        CustomerName = transactionStatus.Card.CardHolderName
                    };

                    eventPublisher.PublishEvent(evt);
                }
                else
                {
                    _log.Warning("CreditCardUsedEvent is not sent!");
                }
            }

            if (transactionStatus.Status == TransactionStatus.Successful)
            {
                tx = await _paymentTransactionsRepository.StartProcessingTransactionAsync(command.TransactionId);

                if (tx != null) // initial status
                {
                    eventPublisher.PublishEvent(new ProcessingStartedEvent
                    {
                        OrderId = command.TransactionId
                    });
                }

                return CommandHandlingResult.Ok();
            }

            await _paymentTransactionsRepository.SetStatusAsync(command.TransactionId, PaymentStatus.NotifyDeclined);

            await _paymentTransactionEventsLog.WriteAsync(
                PaymentTransactionLogEvent.Create(
                    command.TransactionId, command.Request, "Declined by Payment status from payment system", nameof(CashInCommand)));

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(CompleteTransferCommand cmd, IEventPublisher eventPublisher)
        {
            await _paymentTransactionsRepository.SetAsOkAsync(cmd.Id, cmd.Amount, null);

            await _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(cmd.Id, "", "Transaction processed as Ok", nameof(CompleteTransferCommand)));

            eventPublisher.PublishEvent(new TransferCompletedEvent { OrderId = cmd.Id });

            return CommandHandlingResult.Ok();
        }
    }
}
