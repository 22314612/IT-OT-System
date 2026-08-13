using deneme10.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace deneme10.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌟 GİRİŞ SAYFASI (GET)
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer zaten giriş yaptıysa doğrudan Dashboard'a at
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Dashboard));
            }
            return View(); // Views/Home/Login.cshtml dosyasını açar
        }

        // 🌟 GİRİŞ YAPMA İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users
                .Include(u => u.Department)
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Fullname),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("DepartmentId", user.DepartmentId?.ToString() ?? "0")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.Error = "Geçersiz e-posta veya şifre!";
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }
        [Authorize]
        public IActionResult Dashboard()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var deptIdStr = User.Claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value;

            int userId = string.IsNullOrEmpty(userIdStr) ? 0 : int.Parse(userIdStr);
            int departmentId = string.IsNullOrEmpty(deptIdStr) ? 0 : int.Parse(deptIdStr);

            bool isAdmin = User.IsInRole("Admin");
            bool isSupervisor = User.IsInRole("Supervisor");

            var query = _context.Records
                .Include(r => r.User)
                .Include(r => r.TargetDepartment)
                .AsQueryable();

            if (!isAdmin)
            {
                if (isSupervisor)
                {
                    query = query.Where(r => r.TargetDepartmentId == departmentId || r.CreatedByUserId == userId);
                }
                else
                {
                    query = query.Where(r => r.CreatedByUserId == userId);
                }
            }

            DateTime bugun = DateTime.Today;

            // 🌟 Günlük sıfırlanan istatistik sayaçları
            ViewBag.ToplamBekleyen = query.Count(r => r.Status == "İnceleniyor" && r.CreatedAt.Date == bugun);
            ViewBag.BugunIncelenen = query.Count(r => r.CreatedAt.Date == bugun);
            ViewBag.BugunCozulen = query.Count(r => r.Status == "Çözüldü" && r.ResolvedAt != null && r.ResolvedAt.Value.Date == bugun);
            ViewBag.BugunReddedilen = query.Count(r => r.Status == "Reddedildi" && r.ResolvedAt != null && r.ResolvedAt.Value.Date == bugun);

            // 🌟 Doğru değişken adı tanımlandı ve view'a gönderiliyor
            var bekleyenListesi = query
                .Where(r => r.Status == "İnceleniyor")
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(bekleyenListesi);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}