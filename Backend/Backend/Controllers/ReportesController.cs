using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _reportes;

    public ReportesController(ReporteService reportes)
    {
        _reportes = reportes;
    }

    // GET https://localhost:7214/api/reportes/fechas
    // Params: desde=2025-10-01  hasta=2025-10-31
    [HttpGet("fechas")]
    public IActionResult PorFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        return Ok(_reportes.ReportePorFechas(desde, hasta));
    }

    // GET https://localhost:7214/api/reportes/contribuyente
    // Params: nit=7378106  tipo=emitidas (o recibidas)  desde=2025-10-01  hasta=2025-10-31
    [HttpGet("contribuyente")]
    public IActionResult Contribuyente([FromQuery] string nit, [FromQuery] string tipo,
                                       [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        return Ok(_reportes.ConsultarContribuyente(nit, tipo, desde, hasta));
    }

    // GET https://localhost:7214/api/reportes/iva
    // Params: fecha=2025-10-01
    [HttpGet("iva")]
    public IActionResult Iva([FromQuery] DateTime fecha)
    {
        return Ok(_reportes.ResumenIvaPorFecha(fecha));
    }

    // GET https://localhost:7214/api/reportes/rango
    // Params: desde=2025-10-01  hasta=2025-10-31  tipo=total (o sinIva)
    [HttpGet("rango")]
    public IActionResult Rango([FromQuery] DateTime desde, [FromQuery] DateTime hasta,
                               [FromQuery] string tipo)
    {
        return Ok(_reportes.ResumenPorRango(desde, hasta, tipo));
    }
}