using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.Services.Operations.Data
{
    public class SagaState
    {
        [Key]
        public string SagaId { get; set; }

        [Required]
        public string TenantId { get; set; }

        [Required]
        public string SagaType { get; set; }

        [Encrypted]
        public string Data { get; set; }
    }
}