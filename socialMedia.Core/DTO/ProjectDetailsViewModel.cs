using socialMedia.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialMedia.Core.DTO
{
    public class EmployeeTaskDetail
    {
        public int TaskId { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }

        public string Status { get; set; }
        public DateTime Deadline { get; set; }

       

        public string CompletionLink { get; set; }

        // Relations
        public int ProjectId { get; set; }

       
        public string EmployeeName { get; set; } // FK to Identity User


        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CompletionDate { get; set; }

        public bool IsDeleted { get; set; }
    }
    public class TaskWithEmployeeVM
    {
        
        public string Title { get; set; }

        public string Description { get; set; }

        
        public DateTime Deadline { get; set; }

        public string Status { get; set; }

        public string CompletionLink { get; set; } 

        // Relations
        public int ProjectId { get; set; }

        public string ContentType { get; set; }
        public string EmployeeName { get; set; } // FK to Identity User


        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CompletionDate { get; set; }

        public bool IsDeleted { get; set; }
    }
  public  class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public List<TaskWithEmployeeVM> Tasks { get; set; }
    }
}
