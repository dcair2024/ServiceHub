using ServiceHub.Core.Entities;
using ServiceHub.Core.Interfaces;

namespace ServiceHub.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository) => _repository = repository;

    public async Task<Cliente> CriarClienteAsync(string nome, string email)
    {
        if (await _repository.ObterPorEmailAsync(email) != null)
            throw new Exception("E-mail já cadastrado.");

        var cliente = new Cliente(nome, email);
        return await _repository.AdicionarAsync(cliente);
    }

    public async Task DesativarClienteAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id);
        if (cliente == null) throw new Exception("Cliente não encontrado.");
        cliente.Desativar();
        await _repository.AtualizarAsync(cliente);
    }

    public async Task AtivarClienteAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id);
        if (cliente == null) throw new Exception("Cliente não encontrado.");
        cliente.Ativar();
        await _repository.AtualizarAsync(cliente);
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAtivosAsync() =>
        await _repository.ObterTodosAtivosAsync();
}