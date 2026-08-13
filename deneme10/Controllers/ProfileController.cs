using deneme10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace deneme10.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar profiline erişebilir
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Giriş yapan kullanıcının ID'sini (Claim'lerden) alıyoruz
            string userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdString, out int userId))
            {
                // Veritabanından bu ID'ye ait kullanıcıyı buluyoruz
                var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

                // Bulunan kullanıcı verilerini HTML sayfasına (View'a) gönderiyoruz
                return View(user);
            }

            // Eğer bir sorun olursa logine yönlendir
            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public IActionResult Update(User model)
        {
            // Güncellenecek kullanıcıyı veritabanında buluyoruz
            var user = _context.Users.FirstOrDefault(x => x.UserId == model.UserId);

            if (user != null)
            {
                // E-posta adresini güncelliyoruz
                user.Email = model.Email;

                // Eğer şifre kutusu boş bırakılmadıysa (yeni şifre girildiyse) şifreyi de güncelle
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                // Değişiklikleri veritabanına kaydet
                _context.SaveChanges();
            }

            // İşlem bittikten sonra tekrar Profil sayfasına dön
            return RedirectToAction("Index");
        }
    }
}