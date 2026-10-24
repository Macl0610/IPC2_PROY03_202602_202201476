using Backend.Estructuras;

namespace Backend.Models;

// Resumen de valores autorizados por fecha en un rango
public class ResumenRango
{
    public string Tipo { get; set; } = "";              // "total" o "sinIva"
    public ListaEnlazada<ValorDia> Dias { get; set; } = new();
    public decimal TotalRango { get; set; }
}

// Un renglón: el valor autorizado en UNA fecha
public class ValorDia
{
    public string Fecha { get; set; } = "";             // dd/mm/yyyy
    public decimal Valor { get; set; }
}