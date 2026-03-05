namespace ServiceHub.Core.Entities;

public class Cliente
{
    public int Id { get; private set; }

    public string Nome { get; private set; }

    public string Email { get; private set; }

    public bool Ativo { get; private set; }

    // Construtor obrigatório
    public Cliente(string nome, string email)
    {
        Nome = nome;
        Email = email;
        Ativo = true;
    }

    // EF precisa de construtor vazio
    protected Cliente() { }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Atualizar(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }
}