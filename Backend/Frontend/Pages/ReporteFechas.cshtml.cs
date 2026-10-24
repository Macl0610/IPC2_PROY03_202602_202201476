using System.Text.Json;
using Frontend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class ReporteFechasModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public ReporteFechasModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty] public DateTime Desde { get; set; } = DateTime.Today.AddDays(-30);
    [BindProperty] public DateTime Hasta { get; set; } = DateTime.Today;

    public List<ReporteFechaDto>? Resultado { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    // Botón "Consultar"
    public async Task OnPostAsync()
    {
        await Consultar();
    }

    // Botón "Descargar PDF"
    public async Task<IActionResult> OnPostPdfAsync()
    {
        await Consultar();
        if (Resultado == null) return Page();

        var pdf = new GeneradorPdf();
        pdf.Titulo("Reporte de facturas por fecha");
        pdf.Linea($"Del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}");
        pdf.Linea("");

        if (Resultado.Count == 0)
            pdf.Linea("No se recibieron facturas en ese rango.");

        foreach (var r in Resultado)
        {
            pdf.Negrita($"FECHA: {r.Fecha}");
            pdf.Linea($"Cantidad total de facturas recibidas: {r.TotalFacturas}");
            pdf.Linea("Listado de errores en facturas recibidas:");
            pdf.Linea($"     NIT emisor inválido: {r.NitEmisorInvalido}");
            pdf.Linea($"     NIT receptor inválido: {r.NitReceptorInvalido}");
            pdf.Linea($"     NIT emisor inexistente: {r.NitEmisorInexistente}");
            pdf.Linea($"     NIT receptor inexistente: {r.NitReceptorInexistente}");
            pdf.Linea($"     IVA mal calculado: {r.IvaMalCalculado}");
            pdf.Linea($"     TOTAL mal calculado: {r.TotalMalCalculado}");
            pdf.Linea($"     REFERENCIA duplicada: {r.ReferenciaDuplicada}");
            pdf.Linea($"Cantidad de facturas sin errores: {r.FacturasSinErrores}");
            pdf.Linea($"Cantidad de emisores de facturas: {r.CantidadEmisores}");
            pdf.Linea($"Cantidad de receptores de facturas: {r.CantidadReceptores}");
            pdf.Linea("");
        }

        return File(pdf.Generar(), "application/pdf", "ReporteFechas.pdf");
    }

    private async Task Consultar()
    {
        if (Desde > Hasta)
        {
            Error = "La fecha inicial no puede ser mayor que la final";
            return;
        }

        string url = $"api/reportes/fechas?desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}";
        try
        {
            var cliente = _http.CreateClient("Api");
            string json = await cliente.GetStringAsync(url);
            Resultado = JsonSerializer.Deserialize<List<ReporteFechaDto>>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }
}

// Forma del JSON que manda el Backend (un elemento por fecha)
public class ReporteFechaDto
{
    public string Fecha { get; set; } = "";
    public int TotalFacturas { get; set; }
    public int NitEmisorInvalido { get; set; }
    public int NitReceptorInvalido { get; set; }
    public int NitEmisorInexistente { get; set; }
    public int NitReceptorInexistente { get; set; }
    public int IvaMalCalculado { get; set; }
    public int TotalMalCalculado { get; set; }
    public int ReferenciaDuplicada { get; set; }
    public int FacturasSinErrores { get; set; }
    public int CantidadEmisores { get; set; }
    public int CantidadReceptores { get; set; }
}