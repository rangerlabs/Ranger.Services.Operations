using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ranger.Common;

namespace Ranger.Services.Operations.Data
{
    public class SagaLogData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string SagaId { get; set; }

        [Required]
        public string SagaType { get; set; }

        [Encrypted]
        public string Data { get; set; }
    }
}