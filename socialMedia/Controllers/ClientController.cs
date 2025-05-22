using Microsoft.AspNetCore.Mvc;

namespace socialMedia.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ContentProgress()
        {
            return View();
        }

        public IActionResult UpcomimgDeadlines()
        {
            return View();
        }

        public IActionResult Report()
        {
            return View();
        }
    }
}
