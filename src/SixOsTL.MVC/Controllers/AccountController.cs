using Microsoft.AspNetCore.Mvc;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.MVC.Extensions;

namespace SixOsTL.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;
        public AccountController(IAuthService auth) => _auth = auth;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("UserId") is not null) return RedirectAfterLogin();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string tenTK, string matKhau, bool rememberMe = false, string? returnUrl = null, CancellationToken ct = default)
        {
            var result = await _auth.LoginAsync(tenTK, matKhau, ct);
            if (result is null)
            {
                ViewBag.Error = "Tên tài khoản hoặc mật khẩu không đúng, hoặc tài khoản đã hết hiệu lực.";
                ViewBag.TenTK = tenTK;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            var roles = result.Roles.ToArray();
            var hoTen = result.HoTen ?? result.TenTK;          
            HttpContext.Session.SetCurrentUser(result.Id, result.TenTK, roles, hoTen); // lưu vào session
            if (rememberMe)
            {
                var cookieVal = Extensions.SessionExtensions.BuildAuthCookieValue(result.Id, result.TenTK, hoTen, roles);

                Response.Cookies.Append("auth_remember", cookieVal,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,      // true khi deploy HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
            }       
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl); // redirect về trang muốn vào trước đó, hoặc phân luồng theo role
            return RedirectAfterLogin(roles);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("auth_remember"); // xóa cookie khi logout
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectAfterLogin(string[]? roles = null)
        {
            roles ??= HttpContext.Session.GetRoles();
            return roles.Contains("ADMIN") ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "TaiLieu");
        }
    }
}