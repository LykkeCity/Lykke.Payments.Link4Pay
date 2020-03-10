using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Grpc.Core;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Lykke.Sdk;

namespace Lykke.Payments.Link4Pay.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly Server _grpcServer;
        private readonly KeyVaultSettings _keyVaultSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public StartupManager(
            Server grpcServer,
            KeyVaultSettings keyVaultSettings,
            IEncryptionService encryptionService,
            ICqrsEngine cqrsEngine,
            ILogFactory logFactory
            )
        {
            _grpcServer = grpcServer;
            _keyVaultSettings = keyVaultSettings;
            _encryptionService = encryptionService;
            _cqrsEngine = cqrsEngine;
            _log = logFactory.CreateLog(this);
        }

        public async Task StartAsync()
        {
            _grpcServer.Start();

            Console.WriteLine($"Grpc server listening on: {_grpcServer.Ports.First().Host}:{_grpcServer.Ports.First().Port}");

            _log.Info("Initializing certificate...");
            await _encryptionService.InitAsync(_keyVaultSettings.VaultBaseUrl, _keyVaultSettings.CertificateName,
                _keyVaultSettings.PasswordKey);

            _log.Info("Certificate initialized");

            _cqrsEngine.StartPublishers();
            _cqrsEngine.StartSubscribers();
            _cqrsEngine.StartProcesses();
        }
    }
}
