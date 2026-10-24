using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class AyudaModel : PageModel
{
    // Qué opción se muestra: "estudiante" o "documentacion"
    [BindProperty(SupportsGet = true)]
    public string Opcion { get; set; } = "estudiante";

    public void OnGet() { }
}