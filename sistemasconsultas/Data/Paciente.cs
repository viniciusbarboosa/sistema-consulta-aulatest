using System.ComponentModel.DataAnnotations;

namespace sistemasconsultas.Data;

public class Paciente
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(14, ErrorMessage = "CPF inválido.")]
    [CpfValido]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    public DateOnly? DataNascimento { get; set; }

    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")]
    public string? Telefone { get; set; }

    [StringLength(120, ErrorMessage = "O e-mail pode ter no máximo 120 caracteres.")]
    public string? Email { get; set; }
}