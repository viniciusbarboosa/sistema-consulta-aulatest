namespace sistemasconsultas.Services;

public static class TextoExtensions
{
    // Apara os espaços e transforma texto vazio em null
    public static string? NuloSeVazio(this string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrEmpty(texto) ? null : texto;
    }
}