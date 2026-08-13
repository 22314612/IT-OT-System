using deneme10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace deneme10.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DEPARTMAN LİSTESİ (Personeller Dahil)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Users)
                .ToListAsync();

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string departmentName)
        {
            if (!string.IsNullOrWhiteSpace(departmentName))
            {
                var trimmedName = departmentName.Trim();
                bool varMi = await _context.Departments.AnyAsync(d => d.DepartmentName.ToLower() == trimmedName.ToLower());

                if (!varMi)
                {
                    var department = new Department
                    {
                        DepartmentName = trimmedName
                    };

                    _context.Departments.Add(department);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Bu isimde bir departman zaten mevcut!");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Departman adı boş olamaz!");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}