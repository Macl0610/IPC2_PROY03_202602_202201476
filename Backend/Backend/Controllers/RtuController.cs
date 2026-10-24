using Backend.Estructuras;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/rtu")]
public class RtuController : ControllerBase
{
    private readonly RtuService _rtu;

    public RtuController(RtuService rtu)
    {
        _rtu = rtu;
    }

    // POST https://localhost:7214/api/rtu -> carga contribuyentes (JSON en Body)
    [HttpPost]
    public IActionResult Actualizar([FromBody] ListaEnlazada<Contribuyente> contribuyentes)
    {
        return Ok(_rtu.Actualizar(contribuyentes));
    }

    // GET https://localhost:7214/api/rtu -> muestra los contribuyentes guardados
    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_rtu.ObtenerTodos());
    }
}