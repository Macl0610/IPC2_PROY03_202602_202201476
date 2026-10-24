using System.Text.Json;
using Frontend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class ConsultarContribuyenteModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public ConsultarContribuyenteModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty] public string Nit { get; set; } = "";
    [BindProperty] public string Tipo { get; set; } = "emitidas";
    [BindProperty] public DateTime Desde { get; set; } = DateTime.Today.AddDays(-30);
    [BindProperty] public DateTime Hasta { get; set; } = DateTime.Today;

    public ConsultaContribuyenteDto? Resultado { get; set; }
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
        pdf.Titulo("Consulta de contribuyente");
        pdf.Linea($"NIT: {Resultado.Nit}");
        pdf.Linea($"Facturas {Resultado.Tipo}, del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}");
        pdf.Linea("");

        if (Resultado.Dias.Count == 0)
        {
            pdf.Linea("No hay facturas autorizadas en ese rango.");
        }
        else
        {
            pdf.Negrita("Fecha          Cantidad      Monto total");
            foreach (var d in Resultado.Dias)
                pdf.Linea($"{d.Fecha}     {d.CantidadFacturas,8}      {d.MontoTotal,12:0.00}");
            pdf.Negrita($"Total del rango: {Resultado.TotalRango:0.00}");

            pdf.Linea("");
            pdf.Titulo("Listado de facturas");
            foreach (var f in Resultado.Facturas)
            {
                pdf.Linea($"{f.Fecha:dd/MM/yyyy}  Ref: {f.Referencia}  Emisor: {f.NitEmisor}  Receptor: {f.NitReceptor}");
                pdf.Linea($"      Valor: {f.Valor:0.00}  IVA: {f.Iva:0.00}  Total: {f.Total:0.00}  Código: {f.CodigoAprobacion}");
            }
        }

        return File(pdf.Generar(), "application/pdf", "ConsultaContribuyente.pdf");
    }

    private async Task Consultar()
    {
        if (string.IsNullOrWhiteSpace(Nit))
        {
            Error = "Escribe un NIT";
            return;
        }
        if (Desde > Hasta)
        {
            Error = "La fecha inicial no puede ser mayor que la final";
            return;
        }

        string url = $"api/reportes/contribuyente?nit={Uri.EscapeDataString(Nit.Trim())}" +
                     $"&tipo={Tipo}&desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}";

        try
        {
            var cliente = _http.CreateClient("Api");
            string json = await cliente.GetStringAsync(url);
            Resultado = JsonSerializer.Deserialize<ConsultaContribuyenteDto>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }
}

// Clases que copian la forma del JSON que manda el Backend
public class ConsultaContribuyenteDto
{
    public string Nit { get; set; } = "";
    public string Tipo { get; set; } = "";
    public List<ResumenDiaDto> Dias { get; set; } = new();
    public decimal TotalRango { get; set; }
    public List<FacturaDto> Facturas { get; set; } = new();
}

public class ResumenDiaDto
{
    public string Fecha { get; set; } = "";
    public int CantidadFacturas { get; set; }
    public decimal MontoTotal { get; set; }
}

public class FacturaDto
{
    public DateTime Fecha { get; set; }
    public string Referencia { get; set; } = "";
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public long? CodigoAprobacion { get; set; }
}