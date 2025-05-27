using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Infrastructure;
using socialMedia.Shared.DTO;
using static socialMedia.Shared.Enum.Enum;
namespace socialMedia.Controllers
{
    public class AccountAPIController : Controller
    {
        public RoleManager<IdentityRole> RoleManager { get; set; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }

        public readonly ApplicationDbContext _context;

        public AccountAPIController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.RoleManager = roleManager;
            this._context = context;
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest(new { Message = "Role name cannot be empty." });
            }

            var roleExists = await RoleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return BadRequest(new { Message = "Role already exists." });
            }

            var result = await RoleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role '{roleName}' created successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Failed to create role.", Errors = result.Errors });
            }
        }

        [HttpGet]
        [Route("AssingRole")]
        public async Task<IActionResult> AssignRole(string userId, string roleId)
        {

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await RoleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var result = await UserManager.AddToRoleAsync(user, role.NormalizedName);

            if (result.Succeeded)
            {
                return Ok("Role assigned successfully.");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus(string userId, bool status)
        {
            try
            {
                var response = await _context.Users.Where(e => e.Id == userId).FirstOrDefaultAsync();

                if (response != null)
                {
                    response.IsActive = status;
                    _context.Users.Update(response);
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true });
                }
                else
                {
                    return NotFound(new { success = false, message = "User Not Found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }

        }

        [HttpGet("allUsers")]
        public IActionResult GetAllUsers()
        {
            var users = UserManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToList();

            return Ok(users);
        }

        [HttpGet("allRoles")]
        public IActionResult GetAllRoles()
        {
            var roles = RoleManager.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .ToList();

            return Ok(roles);
        }

        [HttpGet("report")]
        public IActionResult GetTaskReport(
     int? year,
     int? month,
     int? projectId,
     string clientId,
     string employeeId,
     string contentType = null,
     string status = null)
        {
            var query = from task in _context.ProjectTasks
                        join project in _context.Projects on task.ProjectId equals project.Id
                        join employee in _context.Users on task.EmployeeId equals employee.Id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where !task.IsDeleted
                        select new
                        {
                            task,
                            project,
                            employee = emp
                        };

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(x => x.task.Status == status);

            if (year.HasValue)
                query = query.Where(x => x.task.CompletionDate.HasValue && x.task.CompletionDate.Value.Year == year);

            if (month.HasValue)
                query = query.Where(x => x.task.CompletionDate.HasValue && x.task.CompletionDate.Value.Month == month);

            if (projectId.HasValue)
                query = query.Where(x => x.project.Id == projectId);

            if (!string.IsNullOrEmpty(contentType))
                query = query.Where(x => x.task.ContentType == contentType);

            if (!string.IsNullOrEmpty(employeeId))
                query = query.Where(x => x.task.EmployeeId == employeeId);

            if (!string.IsNullOrEmpty(clientId))
                query = query.Where(x => x.project.ClientId == clientId);

            // Group and format
            var grouped = query
                .GroupBy(x => x.task.ContentType)
                .Select(g => new
                {
                    ContentTypeValue = g.Key,
                    Count = g.Count()
                })
                .ToList()
                .Select(g => new
                {
                    ContentType = GetContentTypeName(Convert.ToInt32(g.ContentTypeValue)),
                    Count = g.Count
                })
                .ToList();

            return Ok(grouped);
        }

        private static string GetContentTypeName(int contentType)
        {
            return contentType == (int)ContentType.Post ? "Post" :
                   contentType == (int)ContentType.Video ? "Video" :
                   contentType == (int)ContentType.Story ? "Story" :
                   contentType == (int)ContentType.Reel ? "Reel" :
                   contentType == (int)ContentType.Others ? "Other" :
                   "NOT SPECIFIED";
        }

        [HttpGet("api/clients")]
        public async Task<IActionResult> GetClients()
        {
            // Step 1: Load all users
            var users = await _context.Users.ToListAsync();

            // Step 2: Filter in memory using _userManager
            var clients = new List<object>();
            foreach (var user in users)
            {
                if (await UserManager.IsInRoleAsync(user, "Client"))
                {
                    clients.Add(new
                    {
                        id = user.Id,
                        name = user.FirstName + " " + user.LastName
                    });
                }
            }

            return Ok(clients);
        }

        [HttpGet("api/employees")]
        public async Task<IActionResult> GetEmployees()
        {
            // Step 1: Get all users from DB
            var users = await _context.Users.ToListAsync();

            // Step 2: Filter in memory based on role
            var employees = new List<object>();
            foreach (var user in users)
            {
                if (await UserManager.IsInRoleAsync(user, "Employee"))
                {
                    employees.Add(new
                    {
                        Id = user.Id,
                        Name = user.FirstName + " " + user.LastName
                    });
                }
            }

            return Ok(employees);
        }

        [HttpGet("api/projects")]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects
                .Where(p => p.IsActive) // optional: filter only active projects
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    clientName = _context.Users
                        .Where(u => u.Id == p.ClientId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var today = DateTime.Today;

            // Total Projects
            var totalProjects = await _context.Projects.CountAsync(p => p.IsActive);

            // Total Clients (distinct ClientId from active projects)
            var allUsers = await UserManager.Users.ToListAsync();
            var totalClients = 0;

            foreach (var user in allUsers)
            {
                var roles = await UserManager.GetRolesAsync(user);
                if (roles.Contains("Client") && user.IsActive)
                {
                    totalClients++;
                }
            }

            

            // Pending Employees: Active users with "Employee" role
            var pendingEmployees = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                var roles = await UserManager.GetRolesAsync(user);
                if (roles.Contains("Employee") && user.IsActive)
                {
                    pendingEmployees.Add(user);
                }
            }

            var pendingEmployeesCount = pendingEmployees.Count;

            // Pending Approval Requests: Any user who is not active
            var pendingApprovalRequests = allUsers.Count(u => !u.IsActive);

            // Deadline Reminders (not completed and deadline is today or earlier)
            var deadlineReminders = await _context.ProjectTasks
                .Where(t => t.Status != "Completed"  && !t.IsDeleted)
                .CountAsync();

            var summary = new
            {
                totalProjects,
                totalClients,
                totalEmployeesPending = pendingEmployees.Count,
                pendingApprovalRequests,
                deadlineReminders
            };

            return Ok(summary);
        }

    }

}
