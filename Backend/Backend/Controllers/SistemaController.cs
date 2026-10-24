using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/sistema")]
public class SistemaController : ControllerBase
{
    private readonly RtuService _rtu;
    private readonly FacturaService _facturas;

    public SistemaController(RtuService rtu, FacturaService facturas)
    {
        _rtu = rtu;
        _facturas = facturas;
    }

    // POST https://localhost:7214/api/sistema/inicializar -> borra todos los datos (Body: none)
    [HttpPost("inicializar")]
    public IActionResult Inicializar()
    {
        _facturas.Limpiar();
        _rtu.Limpiar();
        return Ok(new { mensaje = "Sistema inicializado" });
    }
}