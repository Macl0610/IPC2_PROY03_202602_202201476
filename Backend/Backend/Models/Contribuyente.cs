using System.Text.Json.Serialization;

namespace Backend.Models;

public class Contribuyente
{
    [JsonPropertyName("NIT")]
    public string Nit { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";
}