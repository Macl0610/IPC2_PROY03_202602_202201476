namespace Backend.Models;

// Resumen de IVA de UNA fecha
public class ResumenIva
{
    public string Fecha { get; set; } = "";               // dd/mm/yyyy
    public List<IvaPorNit> Nits { get; set; } = new();
}

// Un renglón: el IVA de un NIT en esa fecha
public class IvaPorNit
{
    public string Nit { get; set; } = "";
    public decimal IvaEmitido { get; set; }
    public decimal IvaRecibido { get; set; }
}