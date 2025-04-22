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
    public class AccountController : BaseController
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
            return View(new LoginModel());
        }

        // POST: AccountController/SignIn  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginModel model, string? returnURL)
        {
            ViewBag.ReturnUrl = returnURL;

            if (ModelState.IsValid)
            {
                var user = _context.User.FirstOrDefault(u => u.UserName == model.Username && u.Password == model.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                        new Claim("Id", user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Balance", user.Balance.ToString()),
                        new Claim("ImageUrl", user.ImageUrl)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(claimsPrincipal, authProperties);

                    if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                    {
                        return Redirect(returnURL);
                    }

                    if(user.IsAdmin)
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Client");
                    }
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
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
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = model.Password,
                    Balance = 0,
                    ImageUrl = "~/img/user.jpg",
                    CreatedAt = DateTime.Now
                };
                _context.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu vào DB: " + ex.Message);
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }
        #endregion
    }
}
