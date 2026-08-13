using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace deneme10.Models
{
    [Table("Records")]
    public class Record
    {
        public int RecordId { get; set; }
        public string Title { get; set; }
        public string RecordType { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string FeedbackText { get; set; }

        // YENİ EKLENEN TARİH VE SAAT ALANI
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedByUserId { get; set; }
        public int? TargetDepartmentId { get; set; }
        
        // Mevcut özelliklerinin (RecordId, Title, CreatedAt vb.) altına bunları ekle:
        // Çözüm süresini dakika olarak veritabanında tutacak yeni alan
        public double? ResolutionTimeMinutes { get; set; }
        public int? AssignedToUserId { get; set; }     // Talebi üstlenen kişinin ID'si
        public string? AssignedToUserName { get; set; } // Talebi üstlenen kişinin Adı (kolay gösterim için)
        public DateTime? ResolvedAt { get; set; }       // Talebin çözüldüğü tam tarih/saat (Süre hesabı için)

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("CreatedByUserId")]
        public virtual User User { get; set; }
        // PDF veya Görsel Dosya Eki Yolu
        public string? AttachmentPath { get; set; }
        // Aciliyet Durumu (Düşük, Orta, Acil)
        public string Urgency { get; set; } = "Orta";

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("TargetDepartmentId")]
        public virtual Department TargetDepartment { get; set; }
    }
}