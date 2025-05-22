using Microsoft.AspNetCore.Mvc.Rendering;
using socialMedia.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialMedia.Core.DTO
{
    public class ProjectViewModelDTO : Project
    {
        public string? ClientName { get; set; }
    }

    public class ProjectListViewModel
    {
        public List<ProjectViewModelDTO> Projects { get; set; }
        public List<SelectListItem> Clients { get; set; }
    }
}
