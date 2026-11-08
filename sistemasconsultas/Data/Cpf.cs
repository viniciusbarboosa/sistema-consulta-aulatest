using System.ComponentModel.DataAnnotations;

namespace sistemasconsultas.Data;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CpfValidoAttribute : ValidationAttribute
{
    public CpfValidoAttribute() : base("CPF inválido.")
    {
    }

    public override bool IsValid(object? value)
    {
        // O [Required] cuida do campo vazio
        if (value is null)
            return true;

        return Cpf.EhValido(value.ToString());
    }
}

public static class Cpf
{
    public static string Normalizar(string? valor) =>
        new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

    public static string Formatar(string? valor)
    {
        var cpf = Normalizar(valor);

        return cpf.Length == 11
            ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
            : valor ?? string.Empty;
    }

    public static bool EhValido(string? valor)
    {
        var cpf = Normalizar(valor);

        // 11 dígitos e não pode ser uma sequência repetida (111.111.111-11)
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        return cpf[9] - '0' == CalcularDigito(cpf, 9)
            && cpf[10] - '0' == CalcularDigito(cpf, 10);
    }

    private static int CalcularDigito(string cpf, int quantidade)
    {
        var soma = 0;

        for (var i = 0; i < quantidade; i++)
            soma += (cpf[i] - '0') * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}