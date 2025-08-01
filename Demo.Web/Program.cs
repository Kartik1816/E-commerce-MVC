using System.Text;
using Demo.Web.Controllers;
using Demo.Web.Middleware;
using Demo.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient<AuthController>();
builder.Services.AddHttpClient<EditProfileController>();
builder.Services.AddHttpClient<ForgotPasswordController>();
builder.Services.AddHttpClient<VerifyController>();
builder.Services.AddHttpClient<ResetPasswordController>();
builder.Services.AddHttpClient<ChangePassword>();
builder.Services.AddHttpClient<HomeController>();
builder.Services.AddHttpClient<WishListController>();
builder.Services.AddHttpClient<CartController>();
builder.Services.AddHttpClient<OfferController>();
builder.Services.AddHttpClient<PaymentController>();
builder.Services.AddHttpClient<OrdersController>();
builder.Services.AddHttpClient<CategoryController>();
builder.Services.AddHttpClient<InventoryController>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:5214") // Your MVC app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();
app.UseCors("AllowMVC");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
