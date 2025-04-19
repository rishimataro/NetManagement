using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetManagement.Data;
using NetManagement.Helpers;
using NetManagement.Models;

namespace NetManagement.Controllers
{
    public class AccountController : Controller
    {

        private readonly NetManagementContext _context;

        public AccountController(NetManagementContext context)
        {
            _context = context;
        }

        #region SignIn
        // GET: AccountController  
        [HttpGet]
        public IActionResult SignIn(string? returnURL)
        {
            ViewData["Title"] = "SignIn";
            ViewBag.returnURL = returnURL;
            return View(new User());
        }

        // POST: AccountController/SignIn  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(User model, string? returnURL)
        {
            ViewBag.ReturnUrl = returnURL;

            if (ModelState.IsValid)
            {
                var hashedPassword = model.Password.ToMd5Hash(model.UserName);
                var user = _context.User.FirstOrDefault(u => u.UserName == model.UserName && u.Password == hashedPassword);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                        new Claim("Id", user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        // new Claim("Balance", user.Balance.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                    {
                        return Redirect(returnURL);
                    }

                    //return RedirectToAction("Index", "Home");
                    if(user.IsAdmin)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        #endregion

        #region Register
        // GET: AccountController  
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model, IFormFile img)
        {
            try
            {
                _context.Add(model);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu vào DB: " + ex.Message);
            }

            return RedirectToAction("Index", "Home");
            }
        #endregion
    }
}
