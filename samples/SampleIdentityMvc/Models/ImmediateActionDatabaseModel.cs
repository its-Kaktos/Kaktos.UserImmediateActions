using System;
using System.ComponentModel.DataAnnotations;
using Kaktos.UserImmediateActions;

namespace SampleIdentityMvc.Models
{
    public class ImmediateActionDatabaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ActionKey { get; set; }

        public DateTime AddedDate { get; set; }
        public DateTime ExpirationTime { get; set; }
        public AddPurpose Purpose { get; set; }
    }
}