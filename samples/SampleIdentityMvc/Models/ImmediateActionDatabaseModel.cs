using System;
using System.ComponentModel.DataAnnotations;
using Kaktos.UserImmediateActions;

namespace SampleIdentityMvc.Models
{
    public class ImmediateActionDatabaseModel
    {
        [Key] public int Id { get; set; }

        [Required] public string ActionKey { get; set; }

        public DateTimeOffset AddedDateUtc { get; set; }
        public DateTimeOffset ExpirationTimeUtc { get; set; }
        public AddPurpose Purpose { get; set; }
    }
}