using ServiceHub.Core.Entities;
using ServiceHub.Core.Interfaces;

namespace ServiceHub.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Cliente> CriarClienteAsync(string nome, string email)
    {
        // REGRA DE NEGÓCIO: E-mail Único
        var existente = await _clienteRepository.ObterPorEmailAsync(email);
        if (existente != null)
        {
            throw new Exception("Regra de Negócio: Já existe um cliente cadastrado com este e-mail.");
        }

        var novoCliente = new Cliente(nome, email);
        return await _clienteRepository.AdicionarAsync(novoCliente);
    }

    public async Task DesativarClienteAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null) throw new Exception("Cliente não encontrado.");

        cliente.Desativar();
        await _clienteRepository.AtualizarAsync(cliente);
    }

    public async Task AtivarClienteAsync(int id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);
        if (cliente == null) throw new Exception("Cliente não encontrado.");

        cliente.Ativar();
        await _clienteRepository.AtualizarAsync(cliente);
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAtivosAsync()
    {
        return await _clienteRepository.ObterTodosAtivosAsync();
    }
}