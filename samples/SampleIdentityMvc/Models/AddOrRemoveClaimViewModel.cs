using System.Collections.Generic;

namespace SampleIdentityMvc.Models
{
    public class AddOrRemoveClaimViewModel
    {
        public string UserId { get; set; }
        public List<ClaimsViewModel> UserClaims { get; set; }
    }
    
    public class ClaimsViewModel
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public bool IsSelected { get; set; }
    }
}