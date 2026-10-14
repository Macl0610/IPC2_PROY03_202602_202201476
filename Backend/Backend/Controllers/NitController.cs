using Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NitController : ControllerBase
{
    [HttpGet("validar/{nit}")]
    public IActionResult Validar(string nit)
    {
        return Ok(new { nit, valido = ValidadorNit.EsValido(nit) });
    }
}