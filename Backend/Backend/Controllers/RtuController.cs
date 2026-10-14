using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RtuController : ControllerBase
{
    private readonly RtuService _rtu;

    public RtuController(RtuService rtu)
    {
        _rtu = rtu;
    }

    [HttpPost]
    public IActionResult Actualizar([FromBody] List<Contribuyente> contribuyentes)
    {
        return Ok(_rtu.Actualizar(contribuyentes));
    }

    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(_rtu.ObtenerTodos());
    }
}