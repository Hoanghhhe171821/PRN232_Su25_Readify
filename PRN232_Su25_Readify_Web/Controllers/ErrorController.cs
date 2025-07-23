using Microsoft.AspNetCore.Mvc;

namespace PRN232_Su25_Readify_Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
