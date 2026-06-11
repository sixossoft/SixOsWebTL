using Microsoft.OpenApi.Models;
using SixOsTL.Infrastructure.Persistence;
using SixOsTL.MVC.Middleware;
using SixOsTL.MVC.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SixOsTL.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SixOsTL API",
        Version = "v1",
        Description = "API nội bộ — test FTP và các tích hợp"
    });

    // Swagger hỗ trợ upload file (multipart/form-data)
    c.OperationFilter<FileUploadOperationFilter>();
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024;
});

// ── BUILD ────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();

// nếu muốn chỉ dev thì bọc:
if (app.Environment.IsDevelopment()) app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SixOsTL API v1");
    c.RoutePrefix = "swagger";              // truy cập tại: /swagger
    c.DocumentTitle = "SixOsTL — FTP Test";
    c.DefaultModelsExpandDepth(-1);         
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();