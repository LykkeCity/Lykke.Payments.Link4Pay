using System.Threading.Tasks;
using Grpc.Core;
using Lykke.Sdk;

namespace Lykke.Payments.Link4Pay.Services
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly Server _grpcServer;

        public ShutdownManager(
            Server grpcServer)
        {
            _grpcServer = grpcServer;
        }

        public Task StopAsync()
        {
            return _grpcServer.ShutdownAsync();
        }
    }
}
