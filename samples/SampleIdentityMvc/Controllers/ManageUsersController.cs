using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kaktos.UserImmediateActions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleIdentityMvc.Models;

namespace SampleIdentityMvc.Controllers
{
    public class ManageUsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserImmediateActionsService _userImmediateActionsService;

        public ManageUsersController(UserManager<IdentityUser> userManager, IUserImmediateActionsService userImmediateActionsService)
        {
            _userManager = userManager;
            _userImmediateActionsService = userImmediateActionsService;
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
        public async Task<IActionResult> AllUserClaims(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var claims = (await _userManager.GetClaimsAsync(user))
                .Select(c => new AllUserClaimsViewModel
                {
                    ClaimType = c.Type,
                    ClaimValue = c.Value
                }).ToList();

            return View(claims);
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

                if (result.Succeeded)
                {
                    // Call this method when user claims are added successfully.
                    // فقط زمانی این متود رو صدا بزنید که کلیم های کاربر با موفقیت اضافه شده اند.
                    await _userImmediateActionsService.RefreshCookieAsync(model.UserId);
                    return RedirectToAction("AddClaimToUser", new {id = model.UserId});
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> AddClaimToUserWithOutImmediateUpdate(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new AddClaimViewModel {UserId = id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClaimToUserWithOutImmediateUpdate(AddClaimViewModel model)
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

                if (result.Succeeded)
                {
                    return RedirectToAction("AddClaimToUserWithOutImmediateUpdate", new {id = model.UserId});
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}