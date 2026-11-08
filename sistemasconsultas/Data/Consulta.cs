using System.ComponentModel.DataAnnotations;

namespace sistemasconsultas.Data;

public class Consulta
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int MedicoId { get; set; }
    public Medico Medico { get; set; } = null!;

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    public StatusConsulta Status { get; set; } = StatusConsulta.Agendada;

    [StringLength(500, ErrorMessage = "As observações podem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }
}