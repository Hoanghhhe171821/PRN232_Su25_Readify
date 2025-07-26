using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_Su25_Readify_WebAPI.Dtos.Auths;
using PRN232_Su25_Readify_WebAPI.Exceptions;
using PRN232_Su25_Readify_WebAPI.Models;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountsController(IAuthService authService, 
            UserManager<AppUser> userManager,
            IJwtService jwtService,
            RoleManager<IdentityRole> roleManager)
        {
            _authService = authService;
            _userManager = userManager;
            _jwtService = jwtService;
            _roleManager = roleManager;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllAccounts([FromQuery] string? keyword,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var accounts = await _authService.ListAccountsAsync(keyword, page, pageSize);

            return Ok(accounts);
        }

        [HttpGet("get-all-roles")]
        public async Task<IActionResult> GetAllRole()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }

        [HttpPost("save-role")]
        public async Task<IActionResult> SetRoles([FromBody] AssignRoleRequest dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) throw new BRException("User is not found");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var invalidRoles = dto.SelectedRoles.Except(allRoles).ToList();
            if (invalidRoles.Any())
            {
                return BadRequest(new { message = $"Các role không hợp lệ: {string.Join(", ", invalidRoles)}" });
            }
            var resultRemove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var resultAdd = await _userManager.AddToRolesAsync(user, dto.SelectedRoles);

            return Ok("Add Role Successfull");
        }
    }
}
