using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Payments.Link4Pay.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public Link4PaySettings Link4PayService { get; set; }
    }
}
