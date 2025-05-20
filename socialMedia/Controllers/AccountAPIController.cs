using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using socialMedia.Core.Domain;
using socialMedia.Infrastructure;
using socialMedia.Shared.DTO;

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

       

    }

}
