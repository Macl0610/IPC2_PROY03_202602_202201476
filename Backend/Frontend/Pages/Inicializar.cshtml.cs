using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class InicializarModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public InicializarModel(IHttpClientFactory http)
    {
        _http = http;
    }

    public string? Mensaje { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    // Se ejecuta al presionar el botón: borra RTU y facturas en el Backend
    public async Task OnPostAsync()
    {
        var cliente = _http.CreateClient("Api");
        try
        {
            var resp = await cliente.PostAsync("api/sistema/inicializar", null);
            if (resp.IsSuccessStatusCode)
                Mensaje = "Sistema inicializado: se borraron todos los contribuyentes y facturas.";
            else
                Error = $"El Backend respondió {(int)resp.StatusCode}";
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }
}