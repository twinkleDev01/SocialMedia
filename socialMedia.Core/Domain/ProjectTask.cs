using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using socialMedia.Shared.Enum;
namespace socialMedia.Core.Domain
{
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public string Status { get; set; } = "Assigned"; // Assigned, InProgress, Completed

        public string CompletionLink { get; set; } = string.Empty;

        // Relations
        public int ProjectId { get; set; }
        
        public string ContentType { get; set; }
        public string EmployeeId { get; set; } // FK to Identity User
        

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
