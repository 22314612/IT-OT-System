using Microsoft.AspNetCore.Mvc;

namespace deneme10.Controllers
{
    public class SuggestionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}