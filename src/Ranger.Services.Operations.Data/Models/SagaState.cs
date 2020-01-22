using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Chronicle;
using Ranger.Common;

namespace Ranger.Services.Operations.Data
{
    public class SagaState
    {
        [Key]
        public string SagaId { get; set; }

        [Required]
        public string DatabaseUsername { get; set; }

        [Required]
        public string SagaType { get; set; }

        [Encrypted]
        public string Data { get; set; }
    }
}