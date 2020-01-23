namespace Ranger.Services.Operations
{
    public abstract class BaseSagaData
    {
        public string DatabaseUsername { get; set; }
        public string Initiator { get; set; }
        public string Domain { get; set; }
    }
}