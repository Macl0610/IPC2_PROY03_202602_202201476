using System.Text.RegularExpressions;

namespace Backend.Utils;

public static class ExtractorFecha
{
    // Exige dd/mm/yyyy hh:mi (hora obligatoria, 2 dígitos en día, mes, hora y minutos).
    // (?<!\d) y (?!\d) evitan tomar un pedazo de un número más largo, como 123/01/2025.
    private static readonly Regex Patron =
        new(@"(?<!\d)(\d{2})/(\d{2})/(\d{4})\s+(\d{2}):(\d{2})(?!\d)");

    public static DateTime? Extraer(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        Match m = Patron.Match(texto);
        if (!m.Success) return null;

        int dia = int.Parse(m.Groups[1].Value);
        int mes = int.Parse(m.Groups[2].Value);
        int anio = int.Parse(m.Groups[3].Value);
        int hora = int.Parse(m.Groups[4].Value);
        int minuto = int.Parse(m.Groups[5].Value);

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