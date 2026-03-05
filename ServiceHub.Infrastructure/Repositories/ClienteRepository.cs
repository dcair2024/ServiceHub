using Microsoft.EntityFrameworkCore;
using ServiceHub.Core.Entities;
using ServiceHub.Core.Interfaces;
using ServiceHub.Infrastructure.Data;

namespace ServiceHub.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(int id) =>
        await _context.Set<Cliente>().FindAsync(id);

    public async Task<Cliente?> ObterPorEmailAsync(string email) =>
        await _context.Set<Cliente>().FirstOrDefaultAsync(c => c.Email == email);

    public async Task<IEnumerable<Cliente>> ObterTodosAtivosAsync() =>
        await _context.Set<Cliente>().Where(c => c.Ativo).ToListAsync();

    public async Task<Cliente> AdicionarAsync(Cliente cliente)
    {
        await _context.Set<Cliente>().AddAsync(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        _context.Set<Cliente>().Update(cliente);
        await _context.SaveChangesAsync();
    }
}