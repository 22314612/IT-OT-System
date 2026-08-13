using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deneme10.Models
{
    // BAŞINDA 'public' YAZMASI ÇOK ÖNEMLİ, YOKSA DİĞER SAYFALAR GÖREMEZ
    public class DepartmentKeyword
    {
        [Key]
        public int KeywordId { get; set; }

        [Required(ErrorMessage = "Kelime alanı zorunludur.")]
        [Display(Name = "Anahtar Kelime")]
        public string Word { get; set; }

        [Display(Name = "Puan (Ağırlık)")]
        public int Weight { get; set; } = 1;

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}