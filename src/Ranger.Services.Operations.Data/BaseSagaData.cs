namespace Ranger.Services.Operations
{
    public abstract class BaseSagaData
    {
        public string TenantId { get; set; }
        public string Initiator { get; set; }
    }
}