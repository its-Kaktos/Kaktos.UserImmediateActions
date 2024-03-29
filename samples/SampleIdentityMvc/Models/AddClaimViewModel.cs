﻿using System.ComponentModel.DataAnnotations;

namespace SampleIdentityMvc.Models
{
    public class AddClaimViewModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string ClaimType { get; set; }
        
        [Required]
        public string ClaimValue { get; set; }
    }
}