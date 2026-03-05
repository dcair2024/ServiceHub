using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Interfaces;
using ServiceHub.Core.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige estar logado
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    // REGRA: E-mail único é validado dentro do CriarClienteAsync
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ClienteInputModel model)
    {
        try
        {
            var cliente = await _clienteService.CriarClienteAsync(model.Nome, model.Email);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            // Retorna o erro de e-mail duplicado ou regra de negócio
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/desativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desativar(int id)
    {
        await _clienteService.DesativarClienteAsync(id);
        return Ok("Cliente desativado com sucesso.");
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> ObterAtivos()
    {
        var ativos = await _clienteService.ObterTodosAtivosAsync();
        return Ok(ativos);
    }
}

public record ClienteInputModel(string Nome, string Email);