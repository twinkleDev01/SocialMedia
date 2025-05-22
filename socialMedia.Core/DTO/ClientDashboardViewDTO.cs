using socialMedia.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialMedia.Core.DTO
{
   public class ClientDashboardViewDTOClientDashboardViewDTO
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }

        public List<ProjectTask> RecentTasks { get; set; }
    }
}
