using Microsoft.EntityFrameworkCore;
using sistemasconsultas.Data;
using sistemasconsultas.Models;

namespace sistemasconsultas.Services;

public record ResumoPainel(int ConsultasHoje, int ConsultasFuturas, int Pacientes, int Medicos);

public class ConsultaService(ApplicationDbContext db, UsuarioAtual usuario, Relogio relogio)
{
    public async Task<List<Consulta>> ListarAsync(DateOnly? data, int? medicoId, StatusConsulta? status)
    {
        var userId = await usuario.ObterIdAsync();

        var consultas = db.Consultas
            .AsNoTracking()
            .Include(c => c.Medico)
            .Include(c => c.Paciente)
            .Where(c => c.UserId == userId);

        if (data is not null)
        {
            var inicioDia = data.Value.ToDateTime(TimeOnly.MinValue);
            var fimDia = inicioDia.AddDays(1);
            consultas = consultas.Where(c => c.Inicio >= inicioDia && c.Inicio < fimDia);
        }

        if (medicoId is not null)
        {
            var id = medicoId.Value;
            consultas = consultas.Where(c => c.MedicoId == id);
        }

        if (status is not null)
        {
            var st = status.Value;
            consultas = consultas.Where(c => c.Status == st);
        }

        return await consultas.OrderBy(c => c.Inicio).ToListAsync();
    }

    public async Task<Consulta?> ObterAsync(int id)
    {
        var userId = await usuario.ObterIdAsync();

        return await db.Consultas
            .AsNoTracking()
            .Include(c => c.Medico)
            .Include(c => c.Paciente)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<ResumoPainel> ObterResumoAsync()
    {
        var userId = await usuario.ObterIdAsync();

        var agora = relogio.Agora;
        var inicioHoje = relogio.Hoje.ToDateTime(TimeOnly.MinValue);
        var fimHoje = inicioHoje.AddDays(1);

        var hoje = await db.Consultas.CountAsync(c =>
            c.UserId == userId
            && c.Status != StatusConsulta.Cancelada
            && c.Inicio >= inicioHoje && c.Inicio < fimHoje);

        var futuras = await db.Consultas.CountAsync(c =>
            c.UserId == userId
            && c.Status == StatusConsulta.Agendada
            && c.Inicio >= agora);

        var pacientes = await db.Pacientes.CountAsync(p => p.UserId == userId);
        var medicos = await db.Medicos.CountAsync(m => m.UserId == userId);

        return new ResumoPainel(hoje, futuras, pacientes, medicos);
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> AgendarAsync(ConsultaForm form)
    {
        var userId = await usuario.ObterIdAsync();

        var (erro, inicio, fim) = await ValidarAsync(userId, form, ignorarId: null, inicioOriginal: null);
        if (erro is not null)
            return erro;

        db.Consultas.Add(new Consulta
        {
            UserId = userId,
            MedicoId = form.MedicoId!.Value,
            PacienteId = form.PacienteId!.Value,
            Inicio = inicio,
            Fim = fim,
            Status = StatusConsulta.Agendada,
            Observacoes = form.Observacoes.NuloSeVazio()
        });

        await db.SaveChangesAsync();
        return null;
    }

    // Retorna null se deu certo, ou a mensagem de erro
    public async Task<string?> AtualizarAsync(ConsultaForm form)
    {
        var userId = await usuario.ObterIdAsync();

        var consulta = await db.Consultas
            .FirstOrDefaultAsync(c => c.Id == form.Id && c.UserId == userId);
        if (consulta is null)
            return "Consulta não encontrada.";

        if (consulta.Status != StatusConsulta.Agendada)
            return "Somente consultas agendadas podem ser alteradas.";

        var (erro, inicio, fim) = await ValidarAsync(userId, form, ignorarId: consulta.Id, inicioOriginal: consulta.Inicio);
        if (erro is not null)
            return erro;

        consulta.MedicoId = form.MedicoId!.Value;
        consulta.PacienteId = form.PacienteId!.Value;
        consulta.Inicio = inicio;
        consulta.Fim = fim;
        consulta.Observacoes = form.Observacoes.NuloSeVazio();

        await db.SaveChangesAsync();
        return null;
    }

    public Task<string?> CancelarAsync(int id) => AlterarStatusAsync(id, StatusConsulta.Cancelada);

    public Task<string?> ConcluirAsync(int id) => AlterarStatusAsync(id, StatusConsulta.Concluida);

    private async Task<string?> AlterarStatusAsync(int id, StatusConsulta novoStatus)
    {
        var userId = await usuario.ObterIdAsync();

        var consulta = await db.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (consulta is null)
            return "Consulta não encontrada.";

        if (consulta.Status != StatusConsulta.Agendada)
            return "Somente consultas agendadas podem ser alteradas.";

        if (novoStatus == StatusConsulta.Concluida && consulta.Inicio > relogio.Agora)
            return "Só é possível concluir uma consulta que já começou.";

        consulta.Status = novoStatus;
        await db.SaveChangesAsync();
        return null;
    }

    // Regras de negócio do agendamento
    private async Task<(string? Erro, DateTime Inicio, DateTime Fim)> ValidarAsync(
        string userId, ConsultaForm form, int? ignorarId, DateTime? inicioOriginal)
    {
        var medicoId = form.MedicoId!.Value;
        var pacienteId = form.PacienteId!.Value;
        var inicio = form.Data!.Value.ToDateTime(form.Hora!.Value);
        var fim = inicio.AddMinutes(form.DuracaoMinutos);

        // Só exige horário futuro quando o horário é novo ou foi alterado
        if (inicioOriginal != inicio && inicio <= relogio.Agora)
            return ("A data e o horário precisam estar no futuro.", inicio, fim);

        // O médico e o paciente precisam ser do usuário logado
        var medicoExiste = await db.Medicos.AnyAsync(m => m.Id == medicoId && m.UserId == userId);
        if (!medicoExiste)
            return ("Médico não encontrado.", inicio, fim);

        var pacienteExiste = await db.Pacientes.AnyAsync(p => p.Id == pacienteId && p.UserId == userId);
        if (!pacienteExiste)
            return ("Paciente não encontrado.", inicio, fim);

        // Dois intervalos se sobrepõem quando: inicioA < fimB e fimA > inicioB
        var ignorar = ignorarId ?? 0;

        var conflitoMedico = await db.Consultas.AnyAsync(c =>
            c.UserId == userId
            && c.MedicoId == medicoId
            && c.Id != ignorar
            && c.Status != StatusConsulta.Cancelada
            && c.Inicio < fim && c.Fim > inicio);
        if (conflitoMedico)
            return ("O médico já tem uma consulta nesse horário.", inicio, fim);

        var conflitoPaciente = await db.Consultas.AnyAsync(c =>
            c.UserId == userId
            && c.PacienteId == pacienteId
            && c.Id != ignorar
            && c.Status != StatusConsulta.Cancelada
            && c.Inicio < fim && c.Fim > inicio);
        if (conflitoPaciente)
            return ("O paciente já tem uma consulta nesse horário.", inicio, fim);

        return (null, inicio, fim);
    }
}