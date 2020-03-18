using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.Logs;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Payments.Link4Pay.AzureRepositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Contract;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.DomainServices;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ConsoleTools
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var settings = new AppSettings();

            config.Bind(settings);

            var container = BuildContainer(settings);

            var paymentsStorage = container.Resolve<INoSQLTableStorage<PaymentTransactionEntity>>();
            var link4PayService = container.Resolve<ILink4PayApiService>();
            var encryptionService = container.Resolve<IEncryptionService>();
            var cqrsEngine = container.Resolve<ICqrsEngine>();

            Console.WriteLine("Initializing certificate...");
            await encryptionService.InitAsync(settings.KeyVault.VaultBaseUrl, settings.KeyVault.CertificateName,
                settings.KeyVault.PasswordKey);

            Console.WriteLine("Certificate initialized");

            Console.WriteLine("Getting payment transactions...");
            var sb = new StringBuilder();
            sb.AppendLine("ClientId,TransactionId,Status,TransactionStatus");

            foreach (var transactionId in settings.Transactions)
            {
                var payment = (await paymentsStorage.GetDataAsync("BCO", entity => entity.TransactionId == transactionId)).FirstOrDefault();

                if (payment != null && payment.Status != PaymentStatus.NotifyProcessed)
                {
                    await Task.WhenAll(
                        paymentsStorage.MergeAsync(payment.PartitionKey, payment.RowKey, entity =>
                        {
                            entity.Status = PaymentStatus.Processing;
                            entity.AntiFraudStatus = "NotFraud";
                            if (string.IsNullOrEmpty(entity.MeTransactionId))
                                entity.MeTransactionId = Guid.NewGuid().ToString();

                            return entity;
                        }),
                        paymentsStorage.MergeAsync(payment.ClientId, payment.TransactionId, entity =>
                        {
                            entity.Status = PaymentStatus.Processing;
                            entity.AntiFraudStatus = "NotFraud";
                            if (string.IsNullOrEmpty(entity.MeTransactionId))
                                entity.MeTransactionId = Guid.NewGuid().ToString();

                            return entity;
                        })
                    );

                    cqrsEngine.PublishEvent(new ProcessingStartedEvent
                    {
                        OrderId = transactionId
                    }, Link4PayBoundedContext.Name);
                }
            }

            Console.WriteLine("Done!");
        }

        private static IContainer BuildContainer(AppSettings settings)
        {
            var builder = new ContainerBuilder();

            var services = new ServiceCollection();
            services.AddConsoleLykkeLogging();

            builder.Populate(services);

            builder.RegisterInstance(settings);
            builder.RegisterInstance(settings.Link4Pay);
            builder.RegisterInstance(settings.KeyVault);

            builder.RegisterInstance(
                new KeyVaultClient(
                    async (string authority, string resource, string scope) =>
                    {
                        var authContext = new AuthenticationContext(authority);
                        var clientCred =
                            new ClientCredential(settings.KeyVault.ClientId, settings.KeyVault.ClientSecret);
                        var result = await authContext.AcquireTokenAsync(resource, clientCred);
                        if (result == null)
                        {
                            throw new InvalidOperationException("Failed to retrieve access token for Key Vault");
                        }

                        return result.AccessToken;
                    }
                ));

            builder.RegisterType<EncryptionService>()
                .As<IEncryptionService>()
                .SingleInstance();

            builder.RegisterType<Link4PayApiService>()
                .As<ILink4PayApiService>()
                .SingleInstance();

            builder.Register(ctx =>
                AzureTableStorage<PaymentTransactionEntity>.Create(
                    ConstantReloadingManager.From(settings.ClientPersonalInfoConnString),
                    "PaymentTransactions", ctx.Resolve<ILogFactory>())
            ).As<INoSQLTableStorage<PaymentTransactionEntity>>().SingleInstance();

            BuildCqrsEngine(builder, settings.CqrsConnString);
            return builder.Build();
        }

        private static void BuildCqrsEngine(ContainerBuilder builder, string cqrsConnString)
        {
             var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri =cqrsConnString };

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

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

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
                            .PublishingEvents(typeof(ProcessingStartedEvent))
                            .With($"{Link4PayBoundedContext.Name}-events"));

                    engine.StartAll();

                    return engine;
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
