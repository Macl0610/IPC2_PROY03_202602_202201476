using System.Text.Json.Serialization;

namespace Backend.Models;

public class RespuestaAutorizacion
{
    [JsonPropertyName("resultadoAutorizaciones")]
    public ResultadoAutorizaciones ResultadoAutorizaciones { get; set; } = new();
}

public class ResultadoAutorizaciones
{
    [JsonPropertyName("cantidadAutorizacionesAprobadas")]
    public int Aprobadas { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitEmisorInexistente")]
    public int RechazadasEmisorInexistente { get; set; }

    [JsonPropertyName("cantidadAutorizacionesRechazadasPorNitReceptorInexistente")]
    public int RechazadasReceptorInexistente { get; set; }

    [JsonPropertyName("detalleAprobaciones")]
    public List<DetalleAprobacion> DetalleAprobaciones { get; set; } = new();
}

public class DetalleAprobacion
{
    [JsonPropertyName("referencia")]
    public string Referencia { get; set; } = "";

    [JsonPropertyName("codigoAprobacion")]
    public long CodigoAprobacion { get; set; }
}