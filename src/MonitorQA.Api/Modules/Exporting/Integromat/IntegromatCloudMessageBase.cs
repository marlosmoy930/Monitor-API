using MonitorQA.Cloud.Messaging;

namespace MonitorQA.Api.Modules.Exporting.Integromat
{
    public abstract class IntegromatCloudMessageBase : ICloudMessage
    {
        public string Id => CloudMessages.IntegromatId;
    }
}
