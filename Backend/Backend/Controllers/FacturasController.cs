using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _facturas;

    public FacturasController(FacturaService facturas)
    {
        _facturas = facturas;
    }

    // POST api/facturas -> recibe el JSON de solicitudes de autorización
    [HttpPost]
    public IActionResult Autorizar([FromBody] List<SolicitudFactura> solicitudes)
    {
        return Ok(_facturas.Autorizar(solicitudes));
    }

    // GET api/facturas -> lista todas las facturas guardadas (para revisar)
    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_facturas.ObtenerTodas());
    }
}