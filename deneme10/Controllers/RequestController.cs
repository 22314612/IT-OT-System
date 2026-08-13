using deneme10.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace deneme10.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ZAMAN FİLTRELİ ANA LİSTE METODU
        // ==========================================
        public IActionResult Index(string timeFilter = "today")
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            int departmentId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value ?? "0");

            bool isAdmin = User.IsInRole("Admin");
            bool isSupervisor = User.IsInRole("Supervisor");

            // Tüm sorguyu başlatıyoruz
            var query = _context.Records
                .Include(r => r.TargetDepartment)
                .Include(r => r.User)
                    .ThenInclude(u => u.Department)
                .AsQueryable();

            // ROL BAZLI KISITLAMA
            if (!isAdmin)
            {
                if (isSupervisor)
                {
                    string deptName = _context.Departments
                        .Where(d => d.DepartmentId == departmentId)
                        .Select(d => d.DepartmentName)
                        .FirstOrDefault();

                    string redirectSearchStr = $"'{deptName}' departmanından";

                    query = query.Where(r => r.CreatedByUserId == userId ||
                                             r.TargetDepartmentId == departmentId ||
                                             (!string.IsNullOrEmpty(deptName) && r.Content.Contains(redirectSearchStr)));
                }
                else
                {
                    query = query.Where(r => r.CreatedByUserId == userId);
                }
            }

            // ZAMAN BAZLI KISITLAMA (FİLTRELEME)
            DateTime today = DateTime.Now.Date; // Sadece gün bilgisini alır (Saat 00:00:00)

            if (timeFilter == "today")
                query = query.Where(r => r.CreatedAt >= today);
            else if (timeFilter == "1week")
                query = query.Where(r => r.CreatedAt >= today.AddDays(-7));
            else if (timeFilter == "1month")
                query = query.Where(r => r.CreatedAt >= today.AddMonths(-1));
            else if (timeFilter == "3months")
                query = query.Where(r => r.CreatedAt >= today.AddMonths(-3));
            else if (timeFilter == "6months")
                query = query.Where(r => r.CreatedAt >= today.AddMonths(-6));
            else if (timeFilter == "1year")
                query = query.Where(r => r.CreatedAt >= today.AddYears(-1));
            // "all" ise filtre uygulanmaz, hepsi gelir.

            // Hangi filtrenin seçili olduğunu View'a gönderiyoruz
            ViewBag.CurrentTimeFilter = timeFilter;

            var talepler = query.OrderByDescending(r => r.CreatedAt).ToList();

            return View(talepler);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string recordType, string content, string urgency, IFormFile? attachment)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            string attachmentPath = null;

            if (attachment != null && attachment.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(attachment.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachment.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }
                    attachmentPath = "/uploads/" + uniqueFileName;
                }
            }

            int finalDepartmentId = 0;
            string routedDepartmentName = "";
            bool isAutoRouted = false;
            string combinedText = (title + " " + content).ToLower();

            var dbKeywords = await _context.DepartmentKeywords.Include(k => k.Department).ToListAsync();
            var departmentScores = new Dictionary<int, int>();

            foreach (var kw in dbKeywords)
            {
                int index = 0;
                string searchWord = kw.Word.ToLower();

                while ((index = combinedText.IndexOf(searchWord, index)) != -1)
                {
                    if (!departmentScores.ContainsKey(kw.DepartmentId))
                        departmentScores[kw.DepartmentId] = 0;

                    departmentScores[kw.DepartmentId] += kw.Weight;
                    index += searchWord.Length;
                }
            }

            if (departmentScores.Any())
            {
                var bestMatch = departmentScores.OrderByDescending(x => x.Value).First();

                if (bestMatch.Value > 0)
                {
                    finalDepartmentId = bestMatch.Key;
                    isAutoRouted = true;

                    var autoDept = await _context.Departments.FindAsync(finalDepartmentId);
                    if (autoDept != null)
                    {
                        routedDepartmentName = autoDept.DepartmentName;
                    }
                }
            }

            string urgencyText = (urgency == "Acil") ? "ACİL koduyla " : "";

            if (!isAutoRouted)
            {
                var defaultDept = _context.Departments.FirstOrDefault(d => d.DepartmentName.ToLower().Contains("bilgi")) ?? _context.Departments.FirstOrDefault();
                if (defaultDept != null)
                {
                    finalDepartmentId = defaultDept.DepartmentId;
                    routedDepartmentName = defaultDept.DepartmentName;
                    content += $"\n\n--- \n🤖 Sistem Notu: Talebinizde net bir departman tespit edilemediğinden, öncelikle '{routedDepartmentName}' departmanına {urgencyText}gönderilmiştir.";
                }
            }
            else
            {
                content += $"\n\n--- \n🤖 Sistem Notu: Talebiniz içeriğine göre otomatik olarak '{routedDepartmentName}' departmanına {urgencyText}gönderilmiştir.";
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content) && finalDepartmentId > 0)
            {
                var yeniTalep = new Record
                {
                    Title = title,
                    RecordType = recordType ?? "Talep",
                    Content = content,
                    Status = "İnceleniyor",
                    FeedbackText = "İnceleniyor...",
                    CreatedByUserId = userId,
                    TargetDepartmentId = finalDepartmentId,
                    Urgency = urgency ?? "Orta",
                    AttachmentPath = attachmentPath,
                    CreatedAt = DateTime.Now
                };

                _context.Records.Add(yeniTalep);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddFeedback(int recordId, string status, string feedbackText)
        {
            int departmentId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value ?? "0");

            bool isAdmin = User.IsInRole("Admin");
            bool isSupervisor = User.IsInRole("Supervisor");

            if (!isAdmin && !isSupervisor)
            {
                return RedirectToAction("Index");
            }

            var record = _context.Records.FirstOrDefault(r => r.RecordId == recordId);

            if (record != null)
            {
                if (isAdmin || (isSupervisor && record.TargetDepartmentId == departmentId))
                {
                    record.Status = status;
                    record.FeedbackText = feedbackText ?? string.Empty;

                    if ((status == "Çözüldü" || status == "Reddedildi") && record.ResolvedAt == null)
                    {
                        record.ResolvedAt = DateTime.Now;
                        TimeSpan timeDifference = record.ResolvedAt.Value - record.CreatedAt;
                        record.ResolutionTimeMinutes = Math.Round(timeDifference.TotalMinutes, 1);
                    }

                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor")]
        public IActionResult AssignToMe(int recordId)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Bilinmeyen Personel";

            var record = _context.Records.FirstOrDefault(r => r.RecordId == recordId);

            if (record != null && record.AssignedToUserId == null)
            {
                record.AssignedToUserId = userId;
                record.AssignedToUserName = userName;
                record.Status = "İnceleniyor";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor")]
        public IActionResult ReassignDepartment(int recordId, int newDepartmentId)
        {
            var record = _context.Records
                .Include(r => r.TargetDepartment)
                .FirstOrDefault(r => r.RecordId == recordId);

            var newDept = _context.Departments.FirstOrDefault(d => d.DepartmentId == newDepartmentId);

            if (record != null && newDept != null)
            {
                string oldDeptName = record.TargetDepartment?.DepartmentName ?? "Bilinmiyor";

                record.TargetDepartmentId = newDepartmentId;
                record.AssignedToUserId = null;
                record.AssignedToUserName = null;
                record.Status = "İnceleniyor";

                string redirectNote = $"\n\n--- \n🔄 Yönlendirme Notu [{DateTime.Now:dd.MM.yyyy HH:mm}]: Bu talep '{oldDeptName}' departmanından '{newDept.DepartmentName}' departmanına gönderilmiştir.";
                record.Content += redirectNote;

                record.FeedbackText = $"🔄 Talebiniz incelenmiş olup, yetkili tarafından '{oldDeptName}' departmanından '{newDept.DepartmentName}' departmanına gönderilmiştir.";

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var record = _context.Records.FirstOrDefault(r => r.RecordId == id);
            if (record != null)
            {
                _context.Records.Remove(record);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var record = _context.Records
                .Include(r => r.TargetDepartment)
                .Include(r => r.User)
                    .ThenInclude(u => u.Department)
                .FirstOrDefault(r => r.RecordId == id);

            if (record == null)
            {
                return RedirectToAction("Index");
            }

            if (User.IsInRole("Admin") || User.IsInRole("Supervisor"))
            {
                ViewBag.AllDepartments = _context.Departments.ToList();
            }

            return View(record);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Supervisor")]
        public IActionResult DetailedReport(string filter)
        {
            int departmentId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value ?? "0");
            bool isAdmin = User.IsInRole("Admin");

            var query = _context.Records
                .Include(r => r.TargetDepartment)
                .Include(r => r.User)
                    .ThenInclude(u => u.Department)
                .AsQueryable();

            if (!isAdmin)
            {
                string deptName = _context.Departments
                    .Where(d => d.DepartmentId == departmentId)
                    .Select(d => d.DepartmentName)
                    .FirstOrDefault();

                string redirectSearchStr = $"'{deptName}' departmanından";

                query = query.Where(r => r.TargetDepartmentId == departmentId ||
                                         (!string.IsNullOrEmpty(deptName) && r.Content.Contains(redirectSearchStr)));
            }

            DateTime now = DateTime.Now;

            if (filter == "daily")
            {
                DateTime startOfDay = now.Date;
                DateTime endOfDay = startOfDay.AddDays(1);
                query = query.Where(r => r.CreatedAt >= startOfDay && r.CreatedAt < endOfDay);
                ViewBag.ReportTitle = "Günlük Personel Performans ve Analiz Raporu (" + now.ToString("dd.MM.yyyy") + ")";
            }
            else if (filter == "weekly")
            {
                DateTime startOfWeek = now.AddDays(-7);
                query = query.Where(r => r.CreatedAt >= startOfWeek);
                ViewBag.ReportTitle = "Haftalık Personel Performans ve Analiz Raporu";
            }
            else if (filter == "monthly")
            {
                DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1);
                query = query.Where(r => r.CreatedAt >= startOfMonth && r.CreatedAt < endOfMonth);
                ViewBag.ReportTitle = "Aylık Personel Performans ve Analiz Raporu (" + now.ToString("MMMM yyyy") + ")";
            }
            else
            {
                ViewBag.ReportTitle = "Genel Personel Performans ve Analiz Raporu (Tüm Zamanlar)";
            }

            var results = query.OrderByDescending(r => r.CreatedAt).ToList();
            int toplamTalep = results.Count;

            var personelRaporu = results
                .GroupBy(r => r.AssignedToUserName ?? "Atanmadı / Havuzda Bekliyor")
                .Select(g => new {
                    PersonelAdi = g.Key,
                    KarsilananSayisi = g.Count(),
                    Oran = toplamTalep > 0 ? (double)g.Count() / toplamTalep * 100 : 0,
                    Talepler = g.ToList()
                })
                .OrderByDescending(x => x.KarsilananSayisi)
                .ToList();

            ViewBag.TotalCount = toplamTalep;
            ViewBag.SolvedCount = results.Count(r => r.Status == "Çözüldü" || r.Status == "Tamamlandı");
            ViewBag.PendingCount = results.Count(r => r.Status == "İnceleniyor");
            ViewBag.PersonelRaporu = personelRaporu;

            return View(results);
        }
    }
}