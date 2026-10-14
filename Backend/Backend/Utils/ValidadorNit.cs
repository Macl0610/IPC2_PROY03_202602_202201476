using System.Text.RegularExpressions;

namespace Backend.Utils;

public static class ValidadorNit
{
    // 1 a 20 dígitos + un dígito verificador (0-9 o K)
    private static readonly Regex Formato = new(@"^(\d{1,20})([0-9K])$");

    public static bool EsValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit)) return false;
        nit = nit.Trim().ToUpper();

        Match m = Formato.Match(nit);
        if (!m.Success) return false;

        string numeros = m.Groups[1].Value;
        string verificador = m.Groups[2].Value;

        // Posición 1 = verificador, así que el último dígito de "numeros" es la posición 2
        int suma = 0;
        int posicion = 2;
        for (int i = numeros.Length - 1; i >= 0; i--)
        {
            suma += (numeros[i] - '0') * posicion;
            posicion++;
        }

        int resultado = (11 - (suma % 11)) % 11;
        string esperado = resultado == 10 ? "K" : resultado.ToString();
        return esperado == verificador;
    }
}