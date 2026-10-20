using System.Text.Json;
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

    public async Task OnPostAsync()
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