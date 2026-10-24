using Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/nit")]
public class NitController : ControllerBase
{
    // GET https://localhost:7214/api/nit/validar/7378106 -> dice si el NIT es válido (sin Params ni Body)
    [HttpGet("validar/{nit}")]
    public IActionResult Validar(string nit)
    {
        return Ok(new { nit, valido = ValidadorNit.EsValido(nit) });
    }
}