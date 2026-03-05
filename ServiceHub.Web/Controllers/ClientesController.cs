using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige Token JWT
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ClienteRequest request)
    {
        try
        {
            var cliente = await _service.CriarClienteAsync(request.Nome, request.Email);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> ListarAtivos() => Ok(await _service.ObterTodosAtivosAsync());

    [HttpPatch("{id}/desativar")]
    [Authorize(Roles = "Admin")] // Apenas Admins desativam
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _service.DesativarClienteAsync(id);
            return Ok("Cliente desativado.");
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public record ClienteRequest(string Nome, string Email);