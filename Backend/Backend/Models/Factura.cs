namespace Backend.Models;

public class Factura
{
    public DateTime Fecha { get; set; }
    public string Referencia { get; set; } = "";
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    // Código yyyymmdd######## (null si fue rechazada)
    public long? CodigoAprobacion { get; set; }

    // Errores encontrados al validar (vacía si está bien)
    public List<string> Errores { get; set; } = new();

    public bool Aprobada => Errores.Count == 0;
}