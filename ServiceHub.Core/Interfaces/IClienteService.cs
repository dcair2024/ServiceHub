namespace ServiceHub.Core.Interfaces;

using ServiceHub.Core.Entities;



public interface IClienteService
{
    Task<Cliente> CriarClienteAsync(string nome, string email);
    Task DesativarClienteAsync(int id);
    Task AtivarClienteAsync(int id);
    Task<IEnumerable<Cliente>> ObterTodosAtivosAsync();

    // ADICIONE ESTES DOIS PARA A TAREFA [FE-S1-07]
    Task<Cliente?> ObterPorIdAsync(int id);
    Task AtualizarClienteAsync(int id, string nome, string email);
}