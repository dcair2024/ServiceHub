using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Interfaces;

namespace ServiceHub.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    // [FE-S1-05]: Listagem
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var clientes = await _service.ObterTodosAtivosAsync();
        return View(clientes);
    }

    // [FE-S1-06]: Cadastro (GET)
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // [FE-S1-06]: Cadastro (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        try
        {
            await _service.CriarClienteAsync(request.Nome, request.Email);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Erro ao salvar: " + ex.Message);
            return View(request);
        }
    }

    // [FE-S1-07]: Edição (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _service.ObterPorIdAsync(id);
        if (cliente == null) return NotFound();

        var request = new ClienteRequest(cliente.Nome, cliente.Email);
        return View(request);
    }

    // [FE-S1-07]: Edição (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        try
        {
            await _service.AtualizarClienteAsync(id, request.Nome, request.Email);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Erro ao atualizar: " + ex.Message);
            return View(request);
        }
    }

    // [FE-S1-08]: Detalhes
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _service.ObterPorIdAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }
}

// Record movido para dentro do namespace, logo abaixo da classe
public record ClienteRequest(string Nome, string Email);