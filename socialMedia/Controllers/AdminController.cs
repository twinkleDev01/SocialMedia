using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Core.DTO;
using socialMedia.Infrastructure;
using System.Security.Claims;

namespace socialMedia.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            foreach(var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type} : {claim.Value}");
            }
            

            return View();
        }
        public async Task<IActionResult> ListOfAllUsers()
        {
            var users = await _context.Users
         .Select(user => new AllUserDetailDTO
         {
             Id = user.Id,
             FirstName = user.FirstName,
             LastName = user.LastName,
             Email = user.Email,
             IsActive = user.IsActive,
             Roles = (from userRole in _context.UserRoles
                      join role in _context.Roles on userRole.RoleId equals role.Id
                      where userRole.UserId == user.Id
                      select role.Name).ToList()
         })
         .ToListAsync();

            return View(users);
        }

        public IActionResult AssignTask()
        {
            ViewBag.Projects = _context.Projects.ToList();
            ViewBag.Employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignTask(ProjectTask task)
        {
            if (ModelState.IsValid)
            {
                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("TaskList");
            }

            // 👇 You must re-populate ViewBag before returning the view
            ViewBag.Projects = _context.Projects.ToList();
            ViewBag.Employees = _userManager.GetUsersInRoleAsync("Employee").Result;

            return View(task);
        }


        public IActionResult TaskList()
        {
            var tasks = (from task in _context.ProjectTasks
                         join project in _context.Projects on task.ProjectId equals project.Id
                         join employee in _context.Users on task.EmployeeId equals employee.Id
                         select new TaskListToShowDTO
                         {
                             Id = task.Id,
                             Title = task.Title,
                             Description = task.Description,
                             Deadline = task.Deadline,
                             Status = task.Status,
                             CompletionLink = task.CompletionLink,
                             ContentType = task.ContentType,
                             ProjectName = project.Name,
                             EmployeeName = employee.UserName
                         }).ToList();

            return View(tasks);
        }

    }
}
