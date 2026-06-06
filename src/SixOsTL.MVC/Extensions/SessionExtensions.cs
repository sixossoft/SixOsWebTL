namespace SixOsTL.MVC.Extensions
{
    public static class SessionExtensions
    {
        public static void SetCurrentUser(this ISession session, long id, string tenTK, string[] roles, string? hoTen = null)
        {
            session.SetString("UserId", id.ToString());
            session.SetString("TenTK", tenTK);
            session.SetString("HoTen", hoTen ?? tenTK);
            session.SetString("Roles", string.Join(",", roles));
        }
        public static long? GetUserId(this ISession session) => long.TryParse(session.GetString("UserId"), out var id) ? id : null;
        public static string[] GetRoles(this ISession session) => session.GetString("Roles")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        public static bool IsAdmin(this ISession session) => session.GetRoles().Contains("ADMIN");
     
        public static string BuildAuthCookieValue(long id, string tenTK, string hoTen, string[] roles) // tạo cookie value để lưu phiên
        {
            var raw = $"{id}|{tenTK}|{hoTen}|{string.Join(",", roles)}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
        }
    }
}
