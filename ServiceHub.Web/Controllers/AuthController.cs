using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Usuário criado com sucesso");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return Unauthorized("Usuário inválido");

        var result = await _userManager.CheckPasswordAsync(user, password);

        if (!result)
            return Unauthorized("Senha inválida");

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GenerateJwtToken(claims);

        return Ok(new { token });
    }


    [HttpPost("add-role")]
    public async Task<IActionResult> AddRole(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return NotFound("Usuário não encontrado");

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role {role} adicionada ao usuário");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("teste-admin")]
    public IActionResult TesteAdmin()
    {
        return Ok("Admin validado com sucesso 🔥");
    }

    private string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
      
}

