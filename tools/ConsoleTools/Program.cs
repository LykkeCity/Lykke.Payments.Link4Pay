using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Contracts.Payments;
using Lykke.Logs;
using Lykke.Payments.Link4Pay.AzureRepositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.DomainServices;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
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

            Console.WriteLine("Initializing certificate...");
            await encryptionService.InitAsync(settings.KeyVault.VaultBaseUrl, settings.KeyVault.CertificateName,
                settings.KeyVault.PasswordKey);

            Console.WriteLine("Certificate initialized");

            Console.WriteLine("Getting payment transactions...");
            var sb = new StringBuilder();
            sb.AppendLine("ClientId,TransactionId,Status,TransactionStatus");

            await paymentsStorage.GetDataByChunksAsync("BCO", entities =>
            {
                var items = entities
                    .Where(x => x.PaymentSystem == CashInPaymentSystem.Link4Pay && x.Status == PaymentStatus.NotifyDeclined)
                    .ToList();

                foreach (var item in items)
                {
                    var transactionStatus = link4PayService.GetTransactionInfoAsync(item.TransactionId).GetAwaiter().GetResult();
                    sb.AppendLine(
                        $"{item.ClientId},{item.TransactionId},{item.Status},{transactionStatus.OriginalTxnStatus}");
                }
            });


            var filename = $"payment-transactions-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
            Console.WriteLine($"Saving results to {filename}...");

            using (var sw = new StreamWriter(filename))
            {
                sw.Write(sb.ToString());
            }

            Console.WriteLine("Done!");
        }

        private static IContainer BuildContainer(AppSettings settings)
        {
            var builder = new ContainerBuilder();

            ILogFactory logFactory = EmptyLogFactory.Instance;

            builder.RegisterInstance(logFactory);
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
            return builder.Build();
        }
    }
}
