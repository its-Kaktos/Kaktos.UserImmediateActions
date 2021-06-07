﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleIdentityMvc.Models
{
    public class AddOrRemoveClaimViewModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public List<ClaimsViewModel> UserClaims { get; set; }
    }
    
    public class ClaimsViewModel
    {
        [Required]
        public string ClaimType { get; set; }
        
        [Required]
        public string ClaimValue { get; set; }
        
        [Required]
        public bool IsSelected { get; set; }
    }
}