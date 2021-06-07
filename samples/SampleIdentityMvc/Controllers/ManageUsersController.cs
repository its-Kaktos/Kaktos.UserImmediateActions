using Microsoft.AspNetCore.Mvc;

namespace SampleIdentityMvc.Controllers
{
    public class ManageUsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}