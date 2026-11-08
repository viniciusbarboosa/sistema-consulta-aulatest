using Microsoft.EntityFrameworkCore;
using sistemasconsultas.Data;

namespace sistemasconsultas.Services;

public class MedicoService(ApplicationDbContext db, UsuarioAtual usuario)
{
    public async Task<List<Medico>> ListarAsync()
    {
        var userId = await usuario.ObterIdAsync();

        return await db.Medicos
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Nome)
            .ToListAsync();
    }

    public async Task<Medico?> ObterAsync(int id)
    {
        var userId = await usuario.ObterIdAsync();

        return await db.Medicos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> CriarAsync(Medico medico)
    {
        var userId = await usuario.ObterIdAsync();
        var crm = medico.Crm.Trim();

        var duplicado = await db.Medicos.AnyAsync(m => m.UserId == userId && m.Crm == crm);
        if (duplicado)
            return "Já existe um médico cadastrado com esse CRM.";

        db.Medicos.Add(new Medico
        {
            UserId = userId,
            Nome = medico.Nome.Trim(),
            Crm = crm,
            Especialidade = medico.Especialidade.Trim(),
            Telefone = medico.Telefone.NuloSeVazio()
        });

        await db.SaveChangesAsync();
        return null;
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> AtualizarAsync(Medico medico)
    {
        var userId = await usuario.ObterIdAsync();

        var existente = await db.Medicos
            .FirstOrDefaultAsync(m => m.Id == medico.Id && m.UserId == userId);
        if (existente is null)
            return "Médico não encontrado.";

        var crm = medico.Crm.Trim();
        var duplicado = await db.Medicos
            .AnyAsync(m => m.UserId == userId && m.Crm == crm && m.Id != medico.Id);
        if (duplicado)
            return "Já existe outro médico cadastrado com esse CRM.";

        existente.Nome = medico.Nome.Trim();
        existente.Crm = crm;
        existente.Especialidade = medico.Especialidade.Trim();
        existente.Telefone = medico.Telefone.NuloSeVazio();

        await db.SaveChangesAsync();
        return null;
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> ExcluirAsync(int id)
    {
        var userId = await usuario.ObterIdAsync();

        var existente = await db.Medicos
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (existente is null)
            return "Médico não encontrado.";

        var possuiConsultas = await db.Consultas
            .AnyAsync(c => c.MedicoId == id && c.UserId == userId);
        if (possuiConsultas)
            return "Este médico possui consultas registradas e não pode ser excluído.";

        db.Medicos.Remove(existente);
        await db.SaveChangesAsync();
        return null;
    }
}