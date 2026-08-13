using deneme10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace deneme10.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentKeywordController : Controller
    {
        // Veritabanı bağlantısı
        private readonly ApplicationDbContext _context;

        public DepartmentKeywordController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DEPARTMANLARIN LİSTESİ (Ana Ekran)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

            // Hangi departmanda kaç kelime olduğunu saymak için ViewBag kullanıyoruz
            var keywordCounts = await _context.DepartmentKeywords
                .GroupBy(k => k.DepartmentId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.KeywordCounts = keywordCounts;

            return View(departments);
        }

        // 2. BELİRLİ BİR DEPARTMANIN KELİMELERİNİ YÖNETME EKRANI
        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            // Sadece bu departmana ait kelimeleri getir
            var keywords = await _context.DepartmentKeywords
                .Where(k => k.DepartmentId == id)
                .OrderByDescending(k => k.Weight) // Önce VETO'lar, sonra standartlar gelsin
                .ToListAsync();

            ViewBag.Department = department;
            return View(keywords);
        }

        // 3. AYNI SAYFA İÇİNDEN HIZLI KELİME EKLEME
        [HttpPost]
        public async Task<IActionResult> AddKeyword(int departmentId, string word, int weight)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                // Ağırlık limitlerini güvene al (Min 1, Max 10)
                if (weight > 10) weight = 10;
                if (weight < 1) weight = 1;

                var newKeyword = new DepartmentKeyword
                {
                    DepartmentId = departmentId,
                    Word = word.ToLower().Trim(),
                    Weight = weight
                };

                _context.DepartmentKeywords.Add(newKeyword);
                await _context.SaveChangesAsync();
            }

            // Ekleme bitince tekrar aynı departmanın kelime sayfasına dön
            return RedirectToAction(nameof(Manage), new { id = departmentId });
        }

        // 4. SİLME İŞLEMİ
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var keyword = await _context.DepartmentKeywords.FindAsync(id);
            if (keyword != null)
            {
                int deptId = keyword.DepartmentId; // Silmeden önce departman ID'sini hafızaya al

                _context.DepartmentKeywords.Remove(keyword);
                await _context.SaveChangesAsync();

                // Sildikten sonra tekrar aynı departmanın sayfasına dön
                return RedirectToAction(nameof(Manage), new { id = deptId });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}