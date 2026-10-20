namespace Backend.Models;

// Resultado de la consulta de un contribuyente
public class ConsultaContribuyente
{
    public string Nit { get; set; } = "";
    public string Tipo { get; set; } = "";              // "emitidas" o "recibidas"
    public List<ResumenDia> Dias { get; set; } = new();
    public decimal TotalRango { get; set; }             // total de todo el rango
    public List<Factura> Facturas { get; set; } = new(); // el listado de facturas
}

// Un renglón: lo que tuvo el contribuyente en UNA fecha
public class ResumenDia
{
    public string Fecha { get; set; } = "";             // dd/mm/yyyy
    public int CantidadFacturas { get; set; }
    public decimal MontoTotal { get; set; }
}