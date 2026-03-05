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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // ================= REGISTER =================
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email e senha são obrigatórios.");

        var userExists = await _userManager.FindByEmailAsync(email);
        if (userExists != null)
            return BadRequest("Usuário já existe");

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true // Facilita o teste inicial
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Usuário criado com sucesso (sem role)");
    }

    // ================= LOGIN =================
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email e senha são obrigatórios.");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return Unauthorized("Usuário inválido");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
            return Unauthorized("Senha inválida");

        var userRoles = await _userManager.GetRolesAsync(user);

        // AJUSTE: Usando ClaimTypes padrão para o ASP.NET reconhecer as Roles
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
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

    // ================= ADD ROLE =================
    // DICA: Para o primeiro teste, você pode comentar o [Authorize] se não tiver nenhum Admin criado
    ///[Authorize(Roles = "Admin")]
    [HttpPost("add-role")]
    public async Task<IActionResult> AddRole(string email, string role)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
            return BadRequest("Email e role são obrigatórios.");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return NotFound("Usuário não encontrado");

        if (!await _roleManager.RoleExistsAsync(role))
            return BadRequest("Role não existe");

        var alreadyInRole = await _userManager.IsInRoleAsync(user, role);

        if (alreadyInRole)
            return BadRequest("Usuário já possui essa role");

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role {role} adicionada com sucesso 🔥");
    }

    // ================= TESTE ADMIN =================
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
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
