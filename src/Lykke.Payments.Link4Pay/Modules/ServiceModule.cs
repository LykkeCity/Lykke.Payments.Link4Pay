using System;
using Autofac;
using Grpc.Core;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using JetBrains.Annotations;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.DomainServices;
using Lykke.Payments.Link4Pay.Services;
using Lykke.Payments.Link4Pay.Settings;
using Lykke.Payments.Link4Pay.Workflow;
using Lykke.Sdk;
using Lykke.SettingsReader;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using Telegram.Bot;

namespace Lykke.Payments.Link4Pay.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<Link4PayServiceImpl>()
                .AsSelf()
                .WithParameter("supportedCountries", _appSettings.CurrentValue.Link4PayService.SupportedCountries)
                .WithParameter("supportedCurrencies", _appSettings.CurrentValue.Link4PayService.SupportedCurrencies)
                .SingleInstance();

            builder.Register(ctx =>
                {
                    var reflectionServiceImpl = new ReflectionServiceImpl(
                        Link4PayService.Descriptor
                    );
                    return new Server
                    {
                        Services =
                        {
                            Link4PayService.BindService(ctx.Resolve<Link4PayServiceImpl>()),
                            ServerReflection.BindService(reflectionServiceImpl)
                        },
                        Ports =
                        {
                            new ServerPort("0.0.0.0", _appSettings.CurrentValue.Link4PayService.GrpcPort,
                                ServerCredentials.Insecure)
                        }
                    };
                }
            ).SingleInstance();

            builder.RegisterInstance(
                new KeyVaultClient(
                    async (string authority, string resource, string scope) =>
                    {
                        var authContext = new AuthenticationContext(authority);
                        var clientCred =
                            new ClientCredential(_appSettings.CurrentValue.Link4PayService.KeyVault.ClientId, _appSettings.CurrentValue.Link4PayService.KeyVault.ClientSecret);
                        var result = await authContext.AcquireTokenAsync(resource, clientCred);
                        if (result == null)
                        {
                            throw new InvalidOperationException("Failed to retrieve access token for Key Vault");
                        }

                        return result.AccessToken;
                    }
                ));

            builder.RegisterInstance(_appSettings.CurrentValue.Link4PayService.Link4Pay);
            builder.RegisterInstance(_appSettings.CurrentValue.Link4PayService.KeyVault);

            builder.RegisterType<EncryptionService>()
                .As<IEncryptionService>()
                .SingleInstance();

            builder.RegisterType<Link4PayApiService>()
                .As<ILink4PayApiService>()
                .SingleInstance();

            builder.RegisterType<AntiFraudChecker>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.Link4PayService.AntiFraudCheckPaymentPeriod))
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.Link4PayService.AntiFraudCheckRegistrationDateSince))
                .WithParameter("notificationEmail",_appSettings.CurrentValue.Link4PayService.AntiFraudNotificationEmail)
                .WithParameter("chatId", _appSettings.CurrentValue.Link4PayService.Telegram.ChatId);

            builder.RegisterType<PaymentOkEmailSender>()
                .As<IPaymentNotifier>()
                .SingleInstance();

            builder.RegisterInstance(_appSettings.CurrentValue.Link4PayService.Telegram);

            builder.RegisterInstance(
                string.IsNullOrEmpty(_appSettings.CurrentValue.Link4PayService.Telegram.Token)
                ? new Mock<ITelegramBotClient>().Object
                : new TelegramBotClient(_appSettings.CurrentValue.Link4PayService.Telegram.Token)
            ).As<ITelegramBotClient>().SingleInstance();
        }
    }
}
