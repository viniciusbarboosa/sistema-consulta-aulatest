using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using sistemasconsultas.Data;

namespace sistemasconsultas.Services;

public class PacienteService(ApplicationDbContext db, UsuarioAtual usuario, Relogio relogio)
{
    public async Task<List<Paciente>> ListarAsync()
    {
        var userId = await usuario.ObterIdAsync();

        return await db.Pacientes
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Paciente?> ObterAsync(int id)
    {
        var userId = await usuario.ObterIdAsync();

        return await db.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> CriarAsync(Paciente paciente)
    {
        var erro = ValidarRegras(paciente);
        if (erro is not null)
            return erro;

        var userId = await usuario.ObterIdAsync();
        var cpf = Cpf.Normalizar(paciente.Cpf);

        var duplicado = await db.Pacientes.AnyAsync(p => p.UserId == userId && p.Cpf == cpf);
        if (duplicado)
            return "Já existe um paciente cadastrado com esse CPF.";

        db.Pacientes.Add(new Paciente
        {
            UserId = userId,
            Nome = paciente.Nome.Trim(),
            Cpf = cpf,
            DataNascimento = paciente.DataNascimento,
            Telefone = paciente.Telefone.NuloSeVazio(),
            Email = paciente.Email.NuloSeVazio()
        });

        await db.SaveChangesAsync();
        return null;
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> AtualizarAsync(Paciente paciente)
    {
        var erro = ValidarRegras(paciente);
        if (erro is not null)
            return erro;

        var userId = await usuario.ObterIdAsync();

        var existente = await db.Pacientes
            .FirstOrDefaultAsync(p => p.Id == paciente.Id && p.UserId == userId);
        if (existente is null)
            return "Paciente não encontrado.";

        var cpf = Cpf.Normalizar(paciente.Cpf);
        var duplicado = await db.Pacientes
            .AnyAsync(p => p.UserId == userId && p.Cpf == cpf && p.Id != paciente.Id);
        if (duplicado)
            return "Já existe outro paciente cadastrado com esse CPF.";

        existente.Nome = paciente.Nome.Trim();
        existente.Cpf = cpf;
        existente.DataNascimento = paciente.DataNascimento;
        existente.Telefone = paciente.Telefone.NuloSeVazio();
        existente.Email = paciente.Email.NuloSeVazio();

        await db.SaveChangesAsync();
        return null;
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> ExcluirAsync(int id)
    {
        var userId = await usuario.ObterIdAsync();

        var existente = await db.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (existente is null)
            return "Paciente não encontrado.";

        var possuiConsultas = await db.Consultas
            .AnyAsync(c => c.PacienteId == id && c.UserId == userId);
        if (possuiConsultas)
            return "Este paciente possui consultas registradas e não pode ser excluído.";

        db.Pacientes.Remove(existente);
        await db.SaveChangesAsync();
        return null;
    }

    // Regras que os atributos do modelo não cobrem
    private string? ValidarRegras(Paciente paciente)
    {
        var nascimento = paciente.DataNascimento;

        if (nascimento is null || nascimento.Value > relogio.Hoje || nascimento.Value < relogio.Hoje.AddYears(-130))
            return "Data de nascimento inválida.";

        var email = paciente.Email.NuloSeVazio();
        if (email is not null && !new EmailAddressAttribute().IsValid(email))
            return "E-mail inválido.";

        return null;
    }
}