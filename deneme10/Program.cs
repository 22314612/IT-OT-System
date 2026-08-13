using deneme10.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. VERİTABANI BAĞLANTISI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ITPortalDB;Trusted_Connection=True;MultipleActiveResultSets=true"));

// 2. OTURUM YÖNETİMİ
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login"; // 🌟 Giriş yapmayanlar buraya yönlendirilecek
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/Login";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Statik dosyaların yüklenmesi için standart komut

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 3. ROTA AYARI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}"); // İlk açılış Login sayfası olur

app.Run();