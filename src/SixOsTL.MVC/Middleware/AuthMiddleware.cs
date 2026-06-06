using Microsoft.AspNetCore.Http;

namespace SixOsTL.MVC.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] PublicExact =
        [
            "/",
            "/Account/Login",
            "/Account/Logout",
        ];
        private static readonly string[] PublicPrefix =
        [
            "/Home/",
            "/Home",       
            "/css/",
            "/js/",
            "/lib/",
            "/images/",
            "/favicon",
            "/dist/",      
            "/swagger",     
            "/api/ftp-test" 
        ];
        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";
            bool isPublic = PublicExact.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase)) || PublicPrefix.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            if (isPublic)
            {
                await _next(context);
                return;
            }    
            var userId = context.Session.GetString("UserId"); // ktra session
            if (userId is null)
            {
                TryRestoreFromCookie(context);
                userId = context.Session.GetString("UserId");
            }
            if (userId is null)
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                return;
            }
            await _next(context);
        }

        private static void TryRestoreFromCookie(HttpContext context)
        {
            if (!context.Request.Cookies.TryGetValue("auth_remember", out var cookieVal) || string.IsNullOrEmpty(cookieVal)) return;
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieVal));
                // Format: "userId|tenTK|hoTen|roles"
                var parts = decoded.Split('|');
                if (parts.Length < 3) return;
                if (!long.TryParse(parts[0], out _)) return;
                context.Session.SetString("UserId", parts[0]);
                context.Session.SetString("TenTK", parts[1]);
                context.Session.SetString("HoTen", parts[2]);
                context.Session.SetString("Roles", parts.Length > 3 ? parts[3] : "USER");
                context.Response.Cookies.Append("auth_remember", cookieVal, // gia hạn cookie thêm 30 ngày
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,   // true khi deploy HTTPS
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
            }
            catch
            {
                context.Response.Cookies.Delete("auth_remember");
            }
        }
    }
}