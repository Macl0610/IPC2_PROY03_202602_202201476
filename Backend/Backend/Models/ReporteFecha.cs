namespace Backend.Models;

// Un renglón del reporte: lo que pasó en UNA fecha
public class ReporteFecha
{
    public string Fecha { get; set; } = "";          // dd/mm/yyyy
    public int TotalFacturas { get; set; }

    // Listado de errores
    public int NitEmisorInvalido { get; set; }
    public int NitReceptorInvalido { get; set; }
    public int NitEmisorInexistente { get; set; }
    public int NitReceptorInexistente { get; set; }
    public int IvaMalCalculado { get; set; }
    public int TotalMalCalculado { get; set; }
    public int ReferenciaDuplicada { get; set; }

    public int FacturasSinErrores { get; set; }
    public int CantidadEmisores { get; set; }
    public int CantidadReceptores { get; set; }
}