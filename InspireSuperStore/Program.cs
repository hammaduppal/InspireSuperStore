using InspireSuperStore.Areas.Notification.Data;
using InspireSuperStore.Models;
using MainModels.Models;
using MainModels.Util;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    }).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddDbContext<OneDb>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("MarketDB"))
    ;
});
var cookieScheme = CookieAuthenticationDefaults.AuthenticationScheme + builder.Configuration.GetValue<string>("SystemSettings:CookieName");
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = builder.Configuration.GetValue<string>("SystemSettings:CookieName");
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(cookieScheme).AddCookie(cookieScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.LoginPath = new PathString("/Account/Login");

    options.AccessDeniedPath = new PathString("/Account/Failure");

    options.ReturnUrlParameter = "ReturnUrl";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = cookieScheme;
    options.Events.OnValidatePrincipal = context =>
    {
        if (context.Principal == null || !context.Principal.Identity.IsAuthenticated)
        {
            context.RejectPrincipal();
            context.HttpContext.Response.Cookies.Delete(cookieScheme);
        }
        return Task.CompletedTask;
    };
 
    options.Events.OnSigningOut = context =>
    {
        context.HttpContext.Response.Cookies.Delete(cookieScheme);
        return Task.CompletedTask;
    };
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<NotificationService>();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();


app.UseAuthorization();
AppDataUtility.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());
app.UseSession();
app.UseMiddleware<SessionCheckMiddleware>();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");
app.Run();
