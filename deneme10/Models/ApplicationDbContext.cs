using Microsoft.EntityFrameworkCore;
using deneme10.Models;

namespace deneme10.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanındaki tablolarımız
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Record> Records { get; set; }
        // Mevcut DbSet'lerinin altına şunu ekle:
        public DbSet<DepartmentKeyword> DepartmentKeywords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // İsimlendirmeleri ve ilişkileri ayarlıyoruz
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<Record>().ToTable("Records");

            // User ile Department arasındaki ilişkiyi burada bağlıyoruz
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId);

            base.OnModelCreating(modelBuilder);
        }
    }

    // --- KULLANICI MODELİNİ BURAYA TANIMLIYORUZ ---
    public class User
    {
        public int UserId { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        // Departman bağlantısı için eklenen alanlar
        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }
    }
}