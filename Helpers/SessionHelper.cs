namespace NetManagement.Helpers
{
    public static class SessionHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        // This method should be called during application startup to set the IHttpContextAccessor
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private static HttpContext Current => _httpContextAccessor?.HttpContext;

        public static int? UserId => Current?.Session.GetInt32("UserId");
        public static string Role => Current?.Session.GetString("Role");
        public static bool IsAdmin => Role == "Admin";
    }

}
