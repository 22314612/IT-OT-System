using System.Collections.Generic;

namespace deneme10.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; } // Yanına '?' koyduk
        public int? SupervisorId { get; set; } // Sadece 1 tane olmalı

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Record> Records { get; set; }
    }
}