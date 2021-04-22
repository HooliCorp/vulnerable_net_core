using Microsoft.AspNetCore.Mvc;

namespace vulnerable_asp_net_core.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            throw new System.NotImplementedException();
        }

        public IActionResult Privacy()
        {
            throw new System.NotImplementedException();
        }
    }
}