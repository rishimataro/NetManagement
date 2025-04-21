using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NetManagement.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            var user = HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                ViewBag.UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Khách";
                ViewBag.Image = user.FindFirst("UserImage")?.Value ?? "~/images/avatar-cute-3.jpg";
            }
            else
            {
                ViewBag.UserName = "Khách";
                ViewBag.Image = "~/images/avatar-cute-3.jpg";
            }
        }
    }
}
