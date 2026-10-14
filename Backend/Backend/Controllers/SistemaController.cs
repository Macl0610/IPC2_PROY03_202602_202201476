using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SistemaController : ControllerBase
{
    private readonly RtuService _rtu;
    private readonly FacturaService _facturas;

    public SistemaController(RtuService rtu, FacturaService facturas)
    {
        _rtu = rtu;
        _facturas = facturas;
    }

    // POST api/sistema/inicializar -> borra todos los datos
    [HttpPost("inicializar")]
    public IActionResult Inicializar()
    {
        _facturas.Limpiar();
        _rtu.Limpiar();
        return Ok(new { mensaje = "Sistema inicializado" });
    }
}