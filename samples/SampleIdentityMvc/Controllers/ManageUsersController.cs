using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleIdentityMvc.Models;

namespace SampleIdentityMvc.Controllers
{
    public class ManageUsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ManageUsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _userManager.Users
                .Select(u => new IndexViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                }).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddClaimToUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new AddClaimViewModel {UserId = id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClaimToUser(AddClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null) return NotFound();

                var userClaims = await _userManager.GetClaimsAsync(user);
                if (userClaims.Any(c=> c.Type == model.ClaimType && c.Value == model.ClaimValue))
                {
                    ModelState.AddModelError(string.Empty, "Claim already exists");
                    return View(model);
                }
                
                var result = await _userManager.AddClaimAsync(user, new Claim(model.ClaimType, model.ClaimValue));

                if (result.Succeeded) return RedirectToAction("AddClaimToUser", new {id = model.UserId});

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}