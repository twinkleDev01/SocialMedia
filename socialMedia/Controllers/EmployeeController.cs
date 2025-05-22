using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Core.DTO;
using socialMedia.Infrastructure;
using System.Security.Claims;
using static socialMedia.Shared.Enum.Enum;

namespace socialMedia.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyTasks()
        {
            var user = await _userManager.GetUserAsync(User);

            var tasks = await (from t in _context.ProjectTasks
                               join p in _context.Projects on t.ProjectId equals p.Id
                               where t.EmployeeId == user.Id
                               select new EmployeeTaskDetail
                               {
                                   Title = t.Title,
                                   Description = t.Description,
                                   Deadline = t.Deadline,
                                   Status = t.Status,
                                   CompletionLink = t.CompletionLink,
                                   ProjectId = t.ProjectId,
                                   ProjectName = p.Name,
                                   ContentType = Convert.ToInt32(t.ContentType) == (int)ContentType.Post ? "Post" : Convert.ToInt32(t.ContentType) == (int)ContentType.Video ? "Video" : Convert.ToInt32(t.ContentType) == (int)ContentType.Story ? "Story" : Convert.ToInt32(t.ContentType) == (int)ContentType.Reel ? "Reel" : Convert.ToInt32(t.ContentType) == (int)ContentType.Others ? "Other" : "NOT SPECIFIED",
                                   CreatedAt = t.CreatedAt,
                                   UpdatedAt = t.UpdatedAt,
                                   CompletionDate = t.CompletionDate,
                                   IsDeleted = t.IsDeleted,
                                   TaskId = t.Id

                               }).ToListAsync();

            return View(tasks);
        }


        //[HttpPost]
        //public async Task<IActionResult> UpdateStatus(int id, string status, string link)
        //{
        //    var task = await _context.ProjectTasks.FindAsync(id);
        //    if (task != null)
        //    {
        //        task.Status = status;
        //        task.CompletionLink = link;
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction("MyTasks");
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int TaskId, string? Status, string? CompletionLink)
        {
            var task = await _context.ProjectTasks.FindAsync(TaskId);
            if (task == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(Status))
                task.Status = Status;

            if (!string.IsNullOrWhiteSpace(CompletionLink))
                task.CompletionLink = CompletionLink;

            await _context.SaveChangesAsync();

            return RedirectToAction("MyTasks");
        }

        public async Task<IActionResult> MyProjects()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var projects = await _context.ProjectTasks
                .Where(t => t.EmployeeId == userId && !t.IsDeleted)
                .Join(_context.Projects,
                    task => task.ProjectId,
                    project => project.Id,
                    (task, project) => project)
                .Distinct()
                .ToListAsync(); // returns List<Project>


            return View(projects);
        }

    }
}
