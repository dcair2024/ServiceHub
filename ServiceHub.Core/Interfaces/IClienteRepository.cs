using ServiceHub.Core.Entities;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Cliente>> ObterTodosAtivosAsync();
    Task<Cliente> AdicionarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
}
