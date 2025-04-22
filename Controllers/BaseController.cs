using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NetManagement.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            var user = HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                ViewBag.UserName = user.FindFirst(ClaimTypes.Name)?.Value;
                ViewBag.Image = user.FindFirst("ImageUrl")?.Value ?? "~/img/user.jpg";
                ViewBag.IsAdmin = user.IsInRole("Admin");
                ViewBag.UserId = user.FindFirst("Id")?.Value;
                ViewBag.Balance = user.FindFirst("Balance")?.Value;
            }
            else
            {
                ViewBag.UserName = null;
                ViewBag.Image = "~/images/avatar-cute-3.jpg";
                ViewBag.IsAdmin = false;
                ViewBag.UserId = null;
                ViewBag.Balance = "0";
            }
        }
    }
}
