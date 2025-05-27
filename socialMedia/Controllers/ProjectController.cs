using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Core.DTO;
using socialMedia.Infrastructure;
using System.Dynamic;
using static socialMedia.Shared.Enum.Enum;

namespace socialMedia.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class ProjectController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch all projects with their client names (via LINQ join)
            var projects = await (from project in _context.Projects
                                  join user in _context.Users
                                  on project.ClientId equals user.Id into clientGroup
                                  from client in clientGroup.DefaultIfEmpty()
                                  orderby project.CreatedAt descending // 🔥 Sort: newest first
                                  select new ProjectViewModelDTO
                                  {
                                      Id = project.Id,
                                      Name = project.Name,
                                      Description = project.Description,
                                      CreatedAt = project.CreatedAt,
                                      IsActive = project.IsActive,
                                      ClientId = project.ClientId,
                                      ClientName = client != null ? client.FirstName + " " + client.LastName : "No Client"
                                  }).ToListAsync();

            // Get all active clients
            var allActiveUsers = await _userManager.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var clients = new List<ApplicationUser>();
            foreach (var user in allActiveUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Client"))
                {
                    clients.Add(user);
                }
            }

            var clientItems = clients.Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = $"{c.FirstName} {c.LastName}"
            }).ToList();

            var viewModel = new ProjectListViewModel
            {
                Projects = projects,
                Clients = clientItems
            };

            return View(viewModel);
        }
        // GET: Projects/Create
        public IActionResult Create()
        {
            var clientsInRole = _userManager.GetUsersInRoleAsync("Client").Result;
            var activeClients = clientsInRole
                .Where(c => c.IsActive)
                .Select(c => new {
                    c.Id,
                    FullName = c.FirstName + " " + c.LastName
                })
                .ToList();

            ViewBag.Clients = new SelectList(activeClients, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                project.CreatedAt = DateTime.Now;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var clientsInRole = await _userManager.GetUsersInRoleAsync("Client");
            var activeClients = clientsInRole
                .Where(c => c.IsActive)
                .Select(c => new {
                    c.Id,
                    FullName = c.FirstName + " " + c.LastName
                })
                .ToList();

            ViewBag.Clients = new SelectList(activeClients, "Id", "FullName");
            return View(project);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            var tasks = await (from t in _context.ProjectTasks
                               join u in _context.Users on t.EmployeeId equals u.Id
                               where t.ProjectId == id
                               select new TaskWithEmployeeVM
                               {
                                   Title = t.Title,
                                   Description = t.Description,
                                   Deadline = t.Deadline,
                                   Status = t.Status,
                                   CompletionLink = t.CompletionLink,
                                   ProjectId = t.ProjectId,
                                   ContentType = Convert.ToInt32(t.ContentType) == (int)ContentType.Post ? "Post" : Convert.ToInt32(t.ContentType) == (int)ContentType.Video ? "Video" : Convert.ToInt32(t.ContentType) == (int)ContentType.Story ? "Story" : Convert.ToInt32(t.ContentType) == (int)ContentType.Reel ? "Reel" : Convert.ToInt32(t.ContentType) == (int)ContentType.Others ? "Other" : "NOT SPECIFIED",
                                   CreatedAt = t.CreatedAt,
                                   UpdatedAt = t.UpdatedAt,
                                   CompletionDate = t.CompletionDate,
                                   IsDeleted = t.IsDeleted,
                                   EmployeeName = u.FirstName + " " + u.LastName
                               }).ToListAsync();
            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = tasks
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateClient(int projectId, string clientId, [FromForm] bool? isActive)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound();

            project.ClientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId;
            project.IsActive = isActive ?? false;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



    }
}
