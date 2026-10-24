using System.Text.Json;
using Frontend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class ResumenRangoModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public ResumenRangoModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty] public string Tipo { get; set; } = "total";
    [BindProperty] public DateTime Desde { get; set; } = DateTime.Today.AddDays(-30);
    [BindProperty] public DateTime Hasta { get; set; } = DateTime.Today;

    public ResumenRangoDto? Resultado { get; set; }
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
        pdf.Titulo("Resumen por rango de fechas");
        pdf.Linea($"Del {Desde:dd/MM/yyyy} al {Hasta:dd/MM/yyyy}");
        pdf.Linea(Tipo == "sinIva" ? "Valores sin IVA" : "Valores totales (con IVA)");
        pdf.Linea("");

        if (Resultado.Dias.Count == 0)
        {
            pdf.Linea("No hay facturas autorizadas en ese rango.");
        }
        else
        {
            pdf.Negrita("Fecha            Valor");
            foreach (var d in Resultado.Dias)
                pdf.Linea($"{d.Fecha}       {d.Valor:0.00}");
            pdf.Negrita($"Total del rango: {Resultado.TotalRango:0.00}");

            pdf.Linea("");
            pdf.Titulo("Gráfica");
            foreach (var d in Resultado.Dias)
                pdf.Barra(d.Fecha, Porcentaje(d.Valor), d.Valor.ToString("0.00"));
        }

        return File(pdf.Generar(), "application/pdf", "ResumenRango.pdf");
    }

    private async Task Consultar()
    {
        if (Desde > Hasta)
        {
            Error = "La fecha inicial no puede ser mayor que la final";
            return;
        }

        string url = $"api/reportes/rango?desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}&tipo={Tipo}";
        try
        {
            var cliente = _http.CreateClient("Api");
            string json = await cliente.GetStringAsync(url);
            Resultado = JsonSerializer.Deserialize<ResumenRangoDto>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }

    // Para la gráfica: el día con más valor es el 100% del ancho
    public double Porcentaje(decimal valor)
    {
        if (Resultado == null) return 0;

        decimal maximo = 0;
        foreach (var d in Resultado.Dias)
        {
            if (d.Valor > maximo) maximo = d.Valor;
        }

        if (maximo == 0) return 0;
        return (double)(valor / maximo * 100);
    }
}

// Forma del JSON que manda el Backend
public class ResumenRangoDto
{
    public string Tipo { get; set; } = "";
    public List<ValorDiaDto> Dias { get; set; } = new();
    public decimal TotalRango { get; set; }
}

public class ValorDiaDto
{
    public string Fecha { get; set; } = "";
    public decimal Valor { get; set; }
}