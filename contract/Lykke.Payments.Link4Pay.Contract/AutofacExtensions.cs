using Autofac;
using Grpc.Core;
using JetBrains.Annotations;

namespace Lykke.Payments.Link4Pay.Contract
{
    [PublicAPI]
    public static class AutofacExtensions
    {
        /// <summary>
        ///     Registers <see cref="Link4PayService.Link4PayServiceClient" /> in Autofac container using
        ///     <see cref="Link4PayServiceClientSettings" />.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings"><see cref="Link4PayServiceClientSettings" /> client settings.</param>
        public static void RegisterLink4PayClient(this ContainerBuilder builder, Link4PayServiceClientSettings settings)
        {
            builder.Register(ctx =>
            {
                var channel = new Channel(settings.GrpcServiceUrl, SslCredentials.Insecure);
                return new Link4PayService.Link4PayServiceClient(channel);
            }).As<Link4PayService.Link4PayServiceClient>();
        }
    }
}
