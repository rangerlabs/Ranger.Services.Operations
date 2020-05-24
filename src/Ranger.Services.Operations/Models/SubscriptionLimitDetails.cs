namespace Ranger.Services.Operations
{
    public class SubscriptionLimitDetails
    {
        public string PlanId { get; set; }
        public LimitFields Limit { get; set; }
        public bool Active { get; set; }
    }
}