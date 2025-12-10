using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Identity.Controllers
{
    [Area("Orchard.Identity")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager) => _userManager = userManager;

        [HttpPost]
        public async Task<IActionResult> CreateTestUser()
        {
            var u = new IdentityUser { UserName = "test", Email = "test@example.com" };
            var result = await _userManager.CreateAsync(u, "P@ssw0rd!");
            return Ok(result.Succeeded ? "created" : string.Join(",", result.Errors.Select(e => e.Description)));
        }
    }
}
