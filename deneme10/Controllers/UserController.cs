using deneme10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace deneme10.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string searchString)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == email);

            if (currentUser == null) return Content("⚠️ Oturum açan kullanıcı bulunamadı.");

            // Sorguyu hemen List'e çevirmeden IQueryable olarak başlatıyoruz
            IQueryable<User> usersQuery;

            if (currentUser.Role == "Admin")
            {
                usersQuery = _context.Users.Include(u => u.Department);
            }
            else
            {
                usersQuery = _context.Users
                    .Include(u => u.Department)
                    .Where(u => u.DepartmentId == currentUser.DepartmentId);
            }

            // ARAMA ÇUBUĞU FİLTRESİ
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Fullname.Contains(searchString) ||
                    u.Email.Contains(searchString)
                );
            }

            // Arama kelimesini kutunun içinde tutmak için View'a geri gönderiyoruz
            ViewBag.CurrentSearch = searchString;

            return View(usersQuery.ToList());
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == email);

            if (currentUser == null) return RedirectToAction("Index");

            var userToDelete = _context.Users.FirstOrDefault(u => u.UserId == id);

            // Kendi hesabı değilse işleme devam et
            if (userToDelete != null && userToDelete.UserId != currentUser.UserId)
            {
                // 🚨 YENİ GÜVENLİK KİLİDİ: Silinmek istenen kişi "Admin" ise işlemi durdur!
                if (userToDelete.Role == "Admin")
                {
                    return RedirectToAction("Index");
                }

                // --- SİLME YETKİ KONTROLÜ ---
                if (currentUser.Role == "Admin")
                {
                    // Admin, diğer (Admin olmayan) herkesi silebilir
                    _context.Users.Remove(userToDelete);
                    _context.SaveChanges();
                }
                else if (currentUser.Role == "Supervisor" && userToDelete.DepartmentId == currentUser.DepartmentId)
                {
                    // Supervisor kendi departmanındakileri silebilir (Tabii ki o kişi Admin değilse)
                    _context.Users.Remove(userToDelete);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(string fullname, string email, string password, int departmentId, string role)
        {
            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    Fullname = fullname,
                    Email = email,
                    Password = password,
                    DepartmentId = departmentId,
                    Role = role
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                ViewBag.Mesaj = "Personel başarıyla sisteme eklendi!";
            }

            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }
    }
}