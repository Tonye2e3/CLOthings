using Microsoft.AspNetCore.Mvc;

namespace CLOthings.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
