using Microsoft.AspNetCore.Mvc;

namespace CLOthings.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            int id = 0;
            Console.WriteLine(id);
            return View();
        }
    }
}
