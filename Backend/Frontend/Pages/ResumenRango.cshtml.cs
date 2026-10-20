using System.Text.Json;
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

    public async Task OnPostAsync()
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