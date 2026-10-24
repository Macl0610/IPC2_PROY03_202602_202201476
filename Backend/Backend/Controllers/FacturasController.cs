using Backend.Estructuras;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/facturas")]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _facturas;

    public FacturasController(FacturaService facturas)
    {
        _facturas = facturas;
    }

    // POST https://localhost:7214/api/facturas 
    [HttpPost]
    public IActionResult Autorizar([FromBody] ListaEnlazada<SolicitudFactura> solicitudes)
    {
        return Ok(_facturas.Autorizar(solicitudes));
    }

    // GET https://localhost:7214/api/facturas -> muestra todas las facturas guardadas
    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_facturas.ObtenerTodas());
    }
}