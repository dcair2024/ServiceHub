using ServiceHub.Core.Entities;
using ServiceHub.Core.Interfaces;

namespace ServiceHub.Infrastructure.Services; // Verifique se o namespace bate com sua pasta

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    // [FE-S1-05]: Listagem
    public async Task<IEnumerable<Cliente>> ObterTodosAtivosAsync()
    {
        return await _repository.ObterTodosAtivosAsync();
    }

    // [FE-S1-06]: Cadastro
    public async Task<Cliente> CriarClienteAsync(string nome, string email)
    {
        if (await _repository.ObterPorEmailAsync(email) != null)
            throw new Exception("E-mail já cadastrado.");

        var cliente = new Cliente(nome, email);
        return await _repository.AdicionarAsync(cliente);
    }

    // [FE-S1-07]: Edição (A tarefa atual)
    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _repository.ObterPorIdAsync(id);
    }

    public async Task AtualizarClienteAsync(int id, string nome, string email)
    {
        var cliente = await _repository.ObterPorIdAsync(id);
        if (cliente == null) throw new Exception("Cliente não encontrado.");

        cliente.Atualizar(nome, email);
        await _repository.AtualizarAsync(cliente);
    }

    // Outros métodos da interface
    public async Task DesativarClienteAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id);
        if (cliente != null)
        {
            cliente.Desativar();
            await _repository.AtualizarAsync(cliente);
        }
    }

    public async Task AtivarClienteAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id);
        if (cliente != null)
        {
            cliente.Ativar();
            await _repository.AtualizarAsync(cliente);
        }
    }
}