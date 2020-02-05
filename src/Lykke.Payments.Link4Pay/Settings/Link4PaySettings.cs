using JetBrains.Annotations;

namespace Lykke.Payments.Link4Pay.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Link4PaySettings
    {
        public DbSettings Db { get; set; }
    }
}
