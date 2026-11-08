namespace sistemasconsultas.Services;

// Hora "de parede" da clínica. O servidor roda em UTC, por isso não usamos DateTime.Now.
public class Relogio
{
    private static readonly TimeZoneInfo Fuso = ObterFuso();

    public DateTime Agora => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso);

    public DateOnly Hoje => DateOnly.FromDateTime(Agora);

    private static TimeZoneInfo ObterFuso()
    {
        // Linux/Docker usa o nome IANA; o Windows usa o nome próprio dele
        foreach (var id in new[] { "America/Recife", "SA Eastern Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        // O Brasil não tem horário de verão desde 2019: UTC-3 fixo
        return TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "Horário de Brasília", "BRT");
    }
}