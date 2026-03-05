using ServiceHub.Core.Entities;

namespace ServiceHub.Core.Interfaces;

public interface IClienteService
{
    Task<Cliente> CriarClienteAsync(string nome, string email);
    Task DesativarClienteAsync(int id);
    Task AtivarClienteAsync(int id);
    Task<IEnumerable<Cliente>> ObterTodosAtivosAsync();
}
