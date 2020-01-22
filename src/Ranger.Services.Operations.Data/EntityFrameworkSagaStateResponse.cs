using System;
using Chronicle;
using Newtonsoft.Json;

namespace Ranger.Services.Operations.Data
{
    public class EntityFrameworkSagaStateResponse
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Inititor { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RejectedReason { get; set; }
    }
}