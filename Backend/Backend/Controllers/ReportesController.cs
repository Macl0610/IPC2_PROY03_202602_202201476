using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _reportes;

    public ReportesController(ReporteService reportes)
    {
        _reportes = reportes;
    }

    // GET api/reportes/fechas?desde=2025-01-01&hasta=2025-01-31
    [HttpGet("fechas")]
    public IActionResult PorFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        return Ok(_reportes.ReportePorFechas(desde, hasta));
    }

    // GET api/reportes/contribuyente?nit=7378106&tipo=emitidas&desde=2025-01-01&hasta=2025-01-31
    [HttpGet("contribuyente")]
    public IActionResult Contribuyente([FromQuery] string nit, [FromQuery] string tipo,
                                       [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        return Ok(_reportes.ConsultarContribuyente(nit, tipo, desde, hasta));
    }
    // GET api/reportes/iva?fecha=2025-01-15
    [HttpGet("iva")]
    public IActionResult Iva([FromQuery] DateTime fecha)
    {
        return Ok(_reportes.ResumenIvaPorFecha(fecha));
    }
    // GET api/reportes/rango?desde=2025-01-01&hasta=2025-01-31&tipo=total
    [HttpGet("rango")]
    public IActionResult Rango([FromQuery] DateTime desde, [FromQuery] DateTime hasta,
                               [FromQuery] string tipo)
    {
        return Ok(_reportes.ResumenPorRango(desde, hasta, tipo));
    }

}