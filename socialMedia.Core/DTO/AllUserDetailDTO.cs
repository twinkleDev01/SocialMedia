using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socialMedia.Core.DTO
{
   public class AllUserDetailDTO : IdentityUser
    {
       
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public bool IsActive { get; set; } = false;

        public List<string> Roles { get; set; }
    }
}
