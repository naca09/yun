using Microsoft.AspNetCore.Mvc;

namespace Project.Models
{
    public class Customer : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
