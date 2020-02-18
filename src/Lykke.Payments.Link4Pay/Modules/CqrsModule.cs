using System;
using System.Collections.Generic;
using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Payments.Link4Pay.Contract;
using Lykke.Payments.Link4Pay.Contract.Events;
using Lykke.Payments.Link4Pay.Settings;
using Lykke.Payments.Link4Pay.Workflow;
using Lykke.Payments.Link4Pay.Workflow.Commands;
using Lykke.Payments.Link4Pay.Workflow.Events;
using Lykke.SettingsReader;

namespace Lykke.Payments.Link4Pay.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settingsManager;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settingsManager = settingsManager;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var settings = _settingsManager.CurrentValue.Link4PayService;

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = settings.Cqrs.ConnectionString };

            builder.Register(ctx => new MessagingEngine(ctx.Resolve<ILogFactory>(),
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory(ctx.Resolve<ILogFactory>()))).As<IMessagingEngine>().SingleInstance();

            builder.RegisterType<PaymentSaga>()
                .WithParameter(TypedParameter.From(settings.SourceClientId));

            builder.RegisterType<MeCommandHandler>()
                .WithParameter("bankCardFeeClientId", settings.BankCardFeeClientId);
            builder.RegisterType<PaymentCommandHandler>();

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            const string meContext = "me";

            builder.Register(ctx =>
                {
                    var engine = new CqrsEngine(ctx.Resolve<ILogFactory>(),
                        ctx.Resolve<IDependencyResolver>(),
                        ctx.Resolve<IMessagingEngine>(),
                        new DefaultEndpointProvider(),
                        true,
                        Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq",
                            SerializationFormat.ProtoBuf, environment: "lykke")),

                        Register.EventInterceptors(new DefaultEventLoggingInterceptor(ctx.Resolve<ILogFactory>())),

                        Register.BoundedContext(Link4PayBoundedContext.Name)
                            .FailedCommandRetryDelay((long) TimeSpan.FromMinutes(1).TotalMilliseconds)
                            .ListeningCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                            .On("payment-saga-commands")
                            .PublishingEvents(typeof(ProcessingStartedEvent), typeof(TransferCompletedEvent),
                                typeof(CreditCardUsedEvent))
                            .With($"{Link4PayBoundedContext.Name}-events")
                            .WithCommandsHandler<PaymentCommandHandler>(),

                        Register.BoundedContext(meContext)
                            .FailedCommandRetryDelay((long) TimeSpan.FromMinutes(1).TotalMilliseconds)
                            .ListeningCommands(typeof(CreateTransferCommand))
                            .On("payment-saga-commands")
                            .PublishingEvents(typeof(TransferCreatedEvent))
                            .With($"{meContext}-events")
                            .WithCommandsHandler<MeCommandHandler>(),

                        Register.Saga<PaymentSaga>("payment-saga")
                            .ListeningEvents(typeof(ProcessingStartedEvent), typeof(TransferCompletedEvent))
                            .From(Link4PayBoundedContext.Name).On($"{Link4PayBoundedContext.Name}-events")
                            .ListeningEvents(typeof(TransferCreatedEvent))
                            .From(meContext).On($"{meContext}-events")
                            .PublishingCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                            .To(Link4PayBoundedContext.Name).With("payment-saga-commands")
                            .PublishingCommands(typeof(CreateTransferCommand))
                            .To(meContext).With("payment-saga-commands"),

                        Register.DefaultRouting
                            .PublishingCommands(typeof(CashInCommand), typeof(CompleteTransferCommand))
                            .To(Link4PayBoundedContext.Name).With("payment-saga-commands")
                            .PublishingCommands(typeof(CreateTransferCommand))
                            .To(meContext).With("payment-saga-commands"));
                    engine.StartPublishers();
                    return engine;
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
