using System.Globalization;
using System.Text;

namespace Frontend.Utils;

// Generador de PDF propio (sin librerías).
// Arma el archivo a mano: páginas, textos, barras y la tabla de referencias (xref).
public class GeneradorPdf
{
    private const double AnchoPagina = 612;   // tamaño carta, en puntos
    private const double AltoPagina = 792;
    private const double Margen = 50;

    private readonly List<StringBuilder> _paginas = new();
    private StringBuilder _actual = new();
    private double _y;

    public GeneradorPdf()
    {
        NuevaPagina();
    }

    public void Titulo(string texto)
    {
        Espacio(24);
        Texto(Margen, _y, 16, true, texto);
        Espacio(6);
    }

    public void Negrita(string texto)
    {
        Espacio(16);
        Texto(Margen, _y, 11, true, texto);
    }

    public void Linea(string texto)
    {
        Espacio(16);
        Texto(Margen, _y, 11, false, texto);
    }

    // Una barra horizontal para las gráficas (porcentaje de 0 a 100)
    public void Barra(string etiqueta, double porcentaje, string valor)
    {
        Espacio(20);
        Texto(Margen, _y, 10, false, etiqueta);
        double x = Margen + 110;
        double anchoMax = AnchoPagina - Margen - x - 60;
        double ancho = anchoMax * porcentaje / 100;
        _actual.Append($"0.2 0.5 0.9 rg {N(x)} {N(_y - 2)} {N(ancho)} 12 re f 0 0 0 rg\n");
        Texto(x + ancho + 5, _y, 10, false, valor);
    }

    // Arma el archivo completo y lo devuelve como bytes
    public byte[] Generar()
    {
        var objetos = new List<string>();
        int n = _paginas.Count;

        var kids = new StringBuilder();
        for (int i = 0; i < n; i++) kids.Append($"{5 + i * 2} 0 R ");

        objetos.Add("<< /Type /Catalog /Pages 2 0 R >>");
        objetos.Add($"<< /Type /Pages /Kids [{kids}] /Count {n} >>");
        objetos.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        objetos.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        for (int i = 0; i < n; i++)
        {
            int contenido = 6 + i * 2;
            objetos.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(AnchoPagina)} {N(AltoPagina)}] " +
                        $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contenido} 0 R >>");
            string flujo = _paginas[i].ToString();
            int largo = Encoding.Latin1.GetByteCount(flujo);
            objetos.Add($"<< /Length {largo} >>\nstream\n{flujo}endstream");
        }

        var salida = new MemoryStream();
        var posiciones = new List<long>();

        Escribir(salida, "%PDF-1.4\n");
        for (int i = 0; i < objetos.Count; i++)
        {
            posiciones.Add(salida.Position);
            Escribir(salida, $"{i + 1} 0 obj\n{objetos[i]}\nendobj\n");
        }

        long inicioXref = salida.Position;
        Escribir(salida, $"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");
        foreach (long p in posiciones) Escribir(salida, $"{p:D10} 00000 n \n");
        Escribir(salida, $"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF");

        return salida.ToArray();
    }

    private void NuevaPagina()
    {
        _actual = new StringBuilder();
        _paginas.Add(_actual);
        _y = AltoPagina - Margen;
    }

    // Baja el renglón; si ya no cabe, empieza otra página
    private void Espacio(double alto)
    {
        if (_y - alto < Margen) NuevaPagina();
        _y -= alto;
    }

    private void Texto(double x, double y, int tam, bool negrita, string texto)
    {
        string fuente = negrita ? "F2" : "F1";
        _actual.Append($"BT /{fuente} {tam} Tf {N(x)} {N(y)} Td ({Escapar(texto)}) Tj ET\n");
    }

    private static void Escribir(MemoryStream salida, string s)
    {
        byte[] b = Encoding.Latin1.GetBytes(s);
        salida.Write(b, 0, b.Length);
    }

    private static string N(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Escapar(string s) =>
        s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}