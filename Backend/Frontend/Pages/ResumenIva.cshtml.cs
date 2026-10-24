using System.Text.Json;
using Frontend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class ResumenIvaModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public ResumenIvaModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty] public DateTime Fecha { get; set; } = DateTime.Today;

    public ResumenIvaDto? Resultado { get; set; }
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
        pdf.Titulo("Resumen de IVA por fecha");
        pdf.Linea($"Fecha: {Resultado.Fecha}");
        pdf.Linea("");

        if (Resultado.Nits.Count == 0)
        {
            pdf.Linea("No hay facturas autorizadas en esa fecha.");
        }
        else
        {
            pdf.Negrita("NIT              IVA emitido      IVA recibido");
            foreach (var n in Resultado.Nits)
                pdf.Linea($"{n.Nit,-16} {n.IvaEmitido,12:0.00}      {n.IvaRecibido,12:0.00}");

            pdf.Linea("");
            pdf.Titulo("Gráfica");
            foreach (var n in Resultado.Nits)
            {
                pdf.Barra($"{n.Nit} emitido", Porcentaje(n.IvaEmitido), n.IvaEmitido.ToString("0.00"));
                pdf.Barra($"{n.Nit} recibido", Porcentaje(n.IvaRecibido), n.IvaRecibido.ToString("0.00"));
            }
        }

        return File(pdf.Generar(), "application/pdf", "ResumenIva.pdf");
    }

    private async Task Consultar()
    {
        string url = $"api/reportes/iva?fecha={Fecha:yyyy-MM-dd}";
        try
        {
            var cliente = _http.CreateClient("Api");
            string json = await cliente.GetStringAsync(url);
            Resultado = JsonSerializer.Deserialize<ResumenIvaDto>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }

    // Para la gráfica: el valor más grande (emitido o recibido) es el 100%
    public double Porcentaje(decimal valor)
    {
        if (Resultado == null) return 0;

        decimal maximo = 0;
        foreach (var n in Resultado.Nits)
        {
            if (n.IvaEmitido > maximo) maximo = n.IvaEmitido;
            if (n.IvaRecibido > maximo) maximo = n.IvaRecibido;
        }

        if (maximo == 0) return 0;
        return (double)(valor / maximo * 100);
    }
}

// Forma del JSON que manda el Backend
public class ResumenIvaDto
{
    public string Fecha { get; set; } = "";
    public List<IvaPorNitDto> Nits { get; set; } = new();
}

public class IvaPorNitDto
{
    public string Nit { get; set; } = "";
    public decimal IvaEmitido { get; set; }
    public decimal IvaRecibido { get; set; }
}