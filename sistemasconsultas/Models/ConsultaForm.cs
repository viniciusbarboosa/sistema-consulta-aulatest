using System.ComponentModel.DataAnnotations;

namespace sistemasconsultas.Models;

public class ConsultaForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione o médico.")]
    public int? MedicoId { get; set; }

    [Required(ErrorMessage = "Selecione o paciente.")]
    public int? PacienteId { get; set; }

    [Required(ErrorMessage = "Informe a data.")]
    public DateOnly? Data { get; set; }

    [Required(ErrorMessage = "Informe o horário.")]
    public TimeOnly? Hora { get; set; }

    [Range(15, 240, ErrorMessage = "Escolha a duração.")]
    public int DuracaoMinutos { get; set; } = 30;

    [StringLength(500, ErrorMessage = "As observações podem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }
}