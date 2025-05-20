using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Infrastructure;

namespace socialMedia.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //public async Task<IActionResult> MyTasks()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var tasks = _context.ProjectTasks
        //                        .Where(t => t.EmployeeId == user.Id)
        //                        .Include(t => t.Project)
        //                        .ToList();
        //    return View(tasks);
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string link)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task != null)
            {
                task.Status = status;
                task.CompletionLink = link;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyTasks");
        }
    }
}
