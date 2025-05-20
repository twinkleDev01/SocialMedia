using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialMedia.Core.DTO
{
  public  class TaskListToShowDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Deadline { get; set; }

        public string Status { get; set; }

        public string CompletionLink { get; set; }

        public string ContentType { get; set; }

        public string ProjectName { get; set; }

        public string EmployeeName { get; set; }
    }
}
