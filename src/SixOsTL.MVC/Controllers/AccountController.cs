using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.MVC.Extensions;

namespace SixOsTL.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;
        private readonly IEmailService _emailService;
        public AccountController(IAuthService auth, IEmailService emailService)
        {
            _auth = auth;
            _emailService = emailService;
        }

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

            // Nếu không có email, không thể gửi OTP
            if (string.IsNullOrWhiteSpace(result.Email))
            {
                ViewBag.Error = "Tài khoản chưa có email để xác thực OTP. Liên hệ quản trị viên.";
                ViewBag.TenTK = tenTK;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Tạo OTP (6 chữ số), lưu tạm vào Session cùng expiry (UTC)
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP_UserId", result.Id.ToString());
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_Expires", DateTime.UtcNow.AddMinutes(5).ToString("o"));
            HttpContext.Session.SetString("OTP_ReturnUrl", returnUrl ?? string.Empty);
            HttpContext.Session.SetString("OTP_RememberMe", rememberMe ? "1" : "0");
            HttpContext.Session.SetString("OTP_TenTK", result.TenTK ?? string.Empty);
            HttpContext.Session.SetString("OTP_HoTen", result.HoTen ?? result.TenTK);
            HttpContext.Session.SetString("OTP_Email", result.Email ?? string.Empty);
            HttpContext.Session.SetString("OTP_Roles", string.Join(",", result.Roles ?? Array.Empty<string>()));

            // Gửi email OTP (async)
            var subject = "Mã xác thực đăng nhập (OTP) — SixOs";
            var body = $"<p>Xin chào {System.Net.WebUtility.HtmlEncode(result.HoTen ?? result.TenTK)},</p>" +
                       $"<p>Mã xác thực đăng nhập của bạn là: <strong style=\"font-size:1.2rem\">{otp}</strong></p>" +
                       $"<p>Mã có hiệu lực trong 5 phút. Nếu không phải bạn yêu cầu, hãy bỏ qua email này.</p>";
            try
            {
                await _emailService.SendEmailAsync(result.Email, subject, body, ct);
            }
            catch
            {
                // Không lộ lỗi chi tiết cho user. Có thể log ở đây.
            }

            return RedirectToAction("VerifyOtp", "Account");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            // Nếu không có OTP trong session => quay về Login
            if (HttpContext.Session.GetString("OTP_Code") is null) return RedirectToAction("Login");
            ViewBag.EmailSentTo = "(email đã gửi)";
            var tenTK = HttpContext.Session.GetString("OTP_TenTK");
            ViewBag.Message = $"Mã xác thực đã được gửi tới email liên kết với tài khoản {tenTK}.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(string code)
        {
            var stored = HttpContext.Session.GetString("OTP_Code");
            var expires = HttpContext.Session.GetString("OTP_Expires");
            var userIdStr = HttpContext.Session.GetString("OTP_UserId");
            var returnUrl = HttpContext.Session.GetString("OTP_ReturnUrl") ?? string.Empty;
            var rememberMe = HttpContext.Session.GetString("OTP_RememberMe") == "1";
            var tenTK = HttpContext.Session.GetString("OTP_TenTK") ?? string.Empty;
            var hoTen = HttpContext.Session.GetString("OTP_HoTen") ?? tenTK;
            var rolesStr = HttpContext.Session.GetString("OTP_Roles") ?? string.Empty;
            var roles = string.IsNullOrWhiteSpace(rolesStr)
                ? Array.Empty<string>()
                : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (stored is null || userIdStr is null)
            {
                ViewBag.Error = "Phiên xác thực đã hết hoặc không hợp lệ. Vui lòng đăng nhập lại.";
                return View();
            }

            if (!DateTime.TryParse(expires, null, System.Globalization.DateTimeStyles.RoundtripKind, out var exp) || DateTime.UtcNow > exp)
            {
                ClearOtpSession();
                ViewBag.Error = "Mã đã hết hạn. Vui lòng đăng nhập lại hoặc yêu cầu gửi lại mã.";
                return View();
            }

            if (string.Equals(stored, code?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                // Thành công -> hoàn tất đăng nhập
                if (!long.TryParse(userIdStr, out var userId)) { ClearOtpSession(); return RedirectToAction("Login"); }

                // Lưu vào session user (dùng roles đã lưu trước khi tạo OTP)
                HttpContext.Session.SetCurrentUser(userId, tenTK, roles, hoTen);

                // Nếu remember me, build cookie
                if (rememberMe)
                {
                    var cookieVal = Extensions.SessionExtensions.BuildAuthCookieValue(userId, tenTK, hoTen, roles);
                    Response.Cookies.Append("auth_remember", cookieVal,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false, // set true in production over HTTPS
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(30)
                        });
                }

                ClearOtpSession();

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectAfterLogin();
            }

            ViewBag.Error = "Mã xác thực không đúng.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(CancellationToken ct = default)
        {
            var userIdStr = HttpContext.Session.GetString("OTP_UserId");
            var tenTK = HttpContext.Session.GetString("OTP_TenTK");
            if (userIdStr is null) return RedirectToAction("Login");

            var email = HttpContext.Session.GetString("OTP_Email");
            if (string.IsNullOrWhiteSpace(email))
            {
                // cannot resend — fallback
                return RedirectToAction("Login");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP_Code", otp);
            HttpContext.Session.SetString("OTP_Expires", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            var subject = "Mã xác thực đăng nhập (OTP) — SixOs";
            var body = $"<p>Xin chào {System.Net.WebUtility.HtmlEncode(tenTK)},</p>" +
                       $"<p>Mã xác thực đăng nhập của bạn là: <strong style=\"font-size:1.2rem\">{otp}</strong></p>" +
                       $"<p>Mã có hiệu lực trong 5 phút.</p>";
            try
            {
                await _emailService.SendEmailAsync(email, subject, body, ct);
            }
            catch
            {
            }

            return RedirectToAction("VerifyOtp");
        }

        private void ClearOtpSession()
        {
            HttpContext.Session.Remove("OTP_UserId");
            HttpContext.Session.Remove("OTP_Code");
            HttpContext.Session.Remove("OTP_Expires");
            HttpContext.Session.Remove("OTP_ReturnUrl");
            HttpContext.Session.Remove("OTP_RememberMe");
            HttpContext.Session.Remove("OTP_TenTK");
            HttpContext.Session.Remove("OTP_HoTen");
            HttpContext.Session.Remove("OTP_Email");
            HttpContext.Session.Remove("OTP_Roles");
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