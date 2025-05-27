using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Infrastructure;
using System.Security.Claims;
using static socialMedia.Shared.Enum.Enum;
namespace socialMedia.Controllers
{
    [Authorize(Roles = "Client,superadmin")]
    [Route("api/[controller]")]
    [ApiController]
   

    public class ClientAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientAPIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var clientId = _userManager.GetUserId(User);

            // Join ProjectTasks with Projects to filter by ClientId
            var query = from task in _context.ProjectTasks
                        join project in _context.Projects
                        on task.ProjectId equals project.Id
                        where project.ClientId == clientId && !task.IsDeleted
                        select task;

            var totalTasks = await query.CountAsync();
            var completedTasks = await query.CountAsync(t => t.Status == "Completed");
            var inProgressTasks = await query.CountAsync(t => t.Status == "InProgress");
            var pendingTasks = await query.CountAsync(t => t.Status == "Assigned");

            var recentTasks = await query
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .Take(5)
                .Select(t => new
                {
                    t.Title,
                    t.Status,
                    t.Deadline,
                    t.ContentType,
                    t.Description
                })
                .ToListAsync();

            var result = new
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                PendingTasks = pendingTasks,
                RecentTasks = recentTasks
            };

            return Ok(result);
        }
        [HttpGet("filter")]
        public IActionResult GetFilteredTasks(string? contentType, string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tasks = (from t in _context.ProjectTasks
                         join p in _context.Projects on t.ProjectId equals p.Id
                         join u in _context.Users on p.ClientId equals u.Id
                         join ar in _context.UserRoles on u.Id equals ar.UserId
                         join r in _context.Roles on ar.RoleId equals r.Id
                         where !t.IsDeleted
                             //&& (string.IsNullOrEmpty(contentType) || t.ContentType == contentType)
                             && (string.IsNullOrEmpty(status) || t.Status == status)
                             && r.Name == "client"
                             && u.Id == userId
                         select new
                         {
                             t.Id,
                             t.Title,
                             t.Description,
                             ContentType = Convert.ToInt16(t.ContentType) == (int)ContentType.Post ? "Post" :
                                           Convert.ToInt16(t.ContentType) == (int)ContentType.Video ? "Video" :
                                           Convert.ToInt16(t.ContentType) == (int)ContentType.Story ? "Story" :
                                           Convert.ToInt16(t.ContentType) == (int)ContentType.Reel ? "Reel" :
                                           Convert.ToInt16(t.ContentType) == (int)ContentType.Others ? "Other" :
                                           "NOT SPECIFIED",
                             t.Status,
                             t.CompletionLink,
                             ProjectName = p.Name,
                             ClientName = u.UserName,
                             RoleName = r.Name,
                             Deadline = t.Deadline.ToString("yyyy-MM-dd")
                         }).ToList();

            return Ok(tasks);

        }

        [HttpGet("upcoming")]
        public IActionResult GetUpcomingDeadlines()
        {
            var today = DateTime.UtcNow.Date;

            var upcoming = (from task in _context.ProjectTasks
                            join project in _context.Projects
                                on task.ProjectId equals project.Id
                            where task.Deadline.Date >= today && task.Deadline.Date <= today.AddDays(7)
                                  && !task.IsDeleted
                            select new
                            {
                                ProjectName = project.Name,
                                task.Title,
                                task.Description,
                                ContentType = Convert.ToInt16(task.ContentType) == (int)ContentType.Post ? "Post" :
                                           Convert.ToInt16(task.ContentType) == (int)ContentType.Video ? "Video" :
                                           Convert.ToInt16(task.ContentType) == (int)ContentType.Story ? "Story" :
                                           Convert.ToInt16(task.ContentType) == (int)ContentType.Reel ? "Reel" :
                                           Convert.ToInt16(task.ContentType) == (int)ContentType.Others ? "Other" :
                                           "NOT SPECIFIED",
                                Deadline = task.Deadline.ToString("yyyy-MM-dd"),
                                task.Status,
                                task.CompletionLink
                            }).ToList();

            return Ok(upcoming);
        }

        [HttpGet("report")]
        public IActionResult GetTaskReport(int? year, int? month, int? projectId, string contentType = null, string status = null)
        {
            // Get the logged-in client ID from claims
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
                return Unauthorized();

            // Join ProjectTasks with Projects manually
            var query = from task in _context.ProjectTasks
                        join project in _context.Projects
                        on task.ProjectId equals project.Id
                        where !task.IsDeleted && project.ClientId == clientId
                        select task;

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (year.HasValue)
                query = query.Where(t => t.CompletionDate.HasValue && t.CompletionDate.Value.Year == year.Value);

            if (month.HasValue)
                query = query.Where(t => t.CompletionDate.HasValue && t.CompletionDate.Value.Month == month.Value);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (!string.IsNullOrEmpty(contentType))
                query = query.Where(t => t.ContentType == contentType);

            // Group and convert ContentType
            var grouped = query
                .GroupBy(t => t.ContentType)
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



        [HttpGet("assigned-projects")]
        public async Task<IActionResult> GetAssignedProjects()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier); // get logged-in client user ID

            var projects = await _context.Projects
                .Where(p => p.ClientId == clientId && p.IsActive)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Description,
                    CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Ok(projects);
        }



    }
}
