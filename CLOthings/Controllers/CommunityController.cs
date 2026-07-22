using Microsoft.AspNetCore.Mvc;

namespace CLOthings.Controllers
{
    public class CommunityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
