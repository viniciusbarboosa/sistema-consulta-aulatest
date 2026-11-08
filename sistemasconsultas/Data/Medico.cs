using System.ComponentModel.DataAnnotations;

namespace sistemasconsultas.Data;

public class Medico
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CRM.")]
    [StringLength(20, ErrorMessage = "O CRM pode ter no máximo 20 caracteres.")]
    public string Crm { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a especialidade.")]
    [StringLength(80, ErrorMessage = "A especialidade pode ter no máximo 80 caracteres.")]
    public string Especialidade { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
    public string? Telefone { get; set; }
}
