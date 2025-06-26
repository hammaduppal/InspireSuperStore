using System.Configuration;
using MainModels.Util;

namespace InspireSuperStore.Models
{
    public class SessionCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _cookieName;
        public SessionCheckMiddleware(RequestDelegate next,IConfiguration configuration)
        {
            _next = next;
            _cookieName = configuration.GetValue<string>("SystemSettings:CookieName");
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string sessionKey = "SessionManager";
            string path = context.Request.Path.Value?.ToLower();

            // Skip session check for login and validation endpoints
            if (path == "/account/login" || path == "/account/validatelogin/")
            {
                await _next(context);
                return;
            }

            // Check if the user is authenticated
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                // If the user is authenticated but session is null
                if (context.Session == null || context.Session.GetString(sessionKey) == null)
                {
                    // Clear the cookie if it exists
                    if (!string.IsNullOrEmpty(_cookieName))
                    {
                        context.Response.Cookies.Delete(_cookieName);
                    }

                    // Redirect to the login page
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            // Continue the middleware pipeline
            await _next(context);
        }

    }
}
