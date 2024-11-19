using AuthenticationMicroService.Models;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;


    private readonly RoleManager<IdentityRole> _roleManager;

    



    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid login attempt." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized(new { Message = "Invalid login attempt." });

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        });
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new AppUser(model.UserName, model.Email);


        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign a role to the user
            if (!string.IsNullOrEmpty(model.Role))
            {
                // Ensure the role exists
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    if (!roleResult.Succeeded)
                        return BadRequest(new { Message = "Failed to create role." });
                }

                // Assign role to the user
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return Ok(new { Message = "User registered successfully." });
        }

        return BadRequest(result.Errors);
    }





  /*  [Authorize(Roles = "Admin")]*/
    [HttpPost("add-role")]
    public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return NotFound(new { Message = "User not found." });

        // Check if the role exists, create it if not
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
            if (!roleResult.Succeeded)
                return BadRequest(new { Message = "Failed to create role." });
        }

        // Assign the role to the user
        var result = await _userManager.AddToRoleAsync(user, model.Role);

        if (result.Succeeded)
            return Ok(new { Message = "Role assigned successfully." });

        return BadRequest(new { Errors = result.Errors });
    }







}
