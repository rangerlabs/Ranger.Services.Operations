using System;

namespace Ranger.Services.Operations
{
    public class OperationDto
    {
        public Guid Id { get; set; }
        public Guid UserEmail { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Resource { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
    }
}