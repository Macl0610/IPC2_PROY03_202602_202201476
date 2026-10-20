using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class EnviarMensajeModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public EnviarMensajeModel(IHttpClientFactory http)
    {
        _http = http;
    }

    [BindProperty]
    public IFormFile? Archivo { get; set; }

    [BindProperty]
    public string Tipo { get; set; } = "rtu";

    public string? Respuesta { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task OnPostAsync()
    {
        if (Archivo == null || Archivo.Length == 0)
        {
            Error = "Selecciona un archivo .json";
            return;
        }
        if (!Archivo.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Error = "El archivo debe tener extensión .json";
            return;
        }

        // Leer el contenido del archivo
        string contenido;
        using (var lector = new StreamReader(Archivo.OpenReadStream()))
        {
            contenido = await lector.ReadToEndAsync();
        }

        string ruta = Tipo == "facturas" ? "api/facturas" : "api/rtu";
        var cliente = _http.CreateClient("Api");

        try
        {
            var resp = await cliente.PostAsync(ruta,
                new StringContent(contenido, Encoding.UTF8, "application/json"));
            string texto = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                // Mostrar la respuesta ordenada
                using var doc = JsonDocument.Parse(texto);
                Respuesta = JsonSerializer.Serialize(doc.RootElement,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                Error = $"El Backend respondió {(int)resp.StatusCode}: {texto}";
            }
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con el Backend. ¿Está corriendo?";
        }
    }
}