using System.Text.RegularExpressions;

namespace Backend.Utils;

public static class ExtractorFecha
{
    // dd/mm/yyyy y, opcionalmente, hh:mi (en cualquier parte del texto)
    private static readonly Regex Patron =
        new(@"(\d{1,2})/(\d{1,2})/(\d{4})(?:\s+(\d{1,2}):(\d{2}))?");

    public static DateTime? Extraer(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        Match m = Patron.Match(texto);
        if (!m.Success) return null;

        int dia = int.Parse(m.Groups[1].Value);
        int mes = int.Parse(m.Groups[2].Value);
        int anio = int.Parse(m.Groups[3].Value);
        int hora = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
        int minuto = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;

        // Hora en formato 24h: 0 a 23
        if (hora > 23 || minuto > 59) return null;

        try
        {
            return new DateTime(anio, mes, dia, hora, minuto, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null; // fecha imposible, como 31/02/2025
        }
    }
}