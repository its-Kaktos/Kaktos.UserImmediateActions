using System.Collections.Generic;

namespace SampleIdentityMvc.Models
{
    public class AddOrRemoveClaimViewModel
    {
        public AddOrRemoveClaimViewModel()
        {
            UserClaims = new List<ClaimsViewModel>();
        }

        public AddOrRemoveClaimViewModel(string userId, List<ClaimsViewModel> userClaims)
        {
            UserId = userId;
            UserClaims = userClaims;
        }


        public string UserId { get; set; }
        public List<ClaimsViewModel> UserClaims { get; set; }
    }
    
    public class ClaimsViewModel
    {
        public ClaimsViewModel()
        {
        }

        public ClaimsViewModel(string claimType)
        {
            ClaimType = claimType;
        }

        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }
}