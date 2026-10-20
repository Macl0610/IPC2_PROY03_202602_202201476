using System.Globalization;
using Backend.Models;

namespace Backend.Services;

public class ReporteService
{
    private readonly FacturaService _facturas;

    public ReporteService(FacturaService facturas)
    {
        _facturas = facturas;
    }

    // Consulta 1: reporte de cada fecha dentro del rango [desde, hasta]
    public List<ReporteFecha> ReportePorFechas(DateTime desde, DateTime hasta)
    {
        // SortedDictionary deja las fechas ordenadas automáticamente
        var porFecha = new SortedDictionary<DateTime, ReporteFecha>();
        var emisores = new Dictionary<DateTime, HashSet<string>>();
        var receptores = new Dictionary<DateTime, HashSet<string>>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            DateTime dia = f.Fecha.Date; // la fecha sin la hora
            if (dia < desde.Date || dia > hasta.Date) continue;

            if (!porFecha.ContainsKey(dia))
            {
                porFecha[dia] = new ReporteFecha
                {
                    Fecha = dia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                };
                emisores[dia] = new HashSet<string>();
                receptores[dia] = new HashSet<string>();
            }

            ReporteFecha r = porFecha[dia];
            r.TotalFacturas++;

            if (f.Errores.Contains(FacturaService.NitEmisorInvalido)) r.NitEmisorInvalido++;
            if (f.Errores.Contains(FacturaService.NitReceptorInvalido)) r.NitReceptorInvalido++;
            if (f.Errores.Contains(FacturaService.NitEmisorInexistente)) r.NitEmisorInexistente++;
            if (f.Errores.Contains(FacturaService.NitReceptorInexistente)) r.NitReceptorInexistente++;
            if (f.Errores.Contains(FacturaService.IvaMalCalculado)) r.IvaMalCalculado++;
            if (f.Errores.Contains(FacturaService.TotalMalCalculado)) r.TotalMalCalculado++;
            if (f.Errores.Contains(FacturaService.ReferenciaDuplicada)) r.ReferenciaDuplicada++;

            if (f.Aprobada)
            {
                r.FacturasSinErrores++;
                emisores[dia].Add(f.NitEmisor);
                receptores[dia].Add(f.NitReceptor);
            }
        }

        foreach (var dia in porFecha.Keys)
        {
            porFecha[dia].CantidadEmisores = emisores[dia].Count;
            porFecha[dia].CantidadReceptores = receptores[dia].Count;
        }

        return porFecha.Values.ToList();
    }
    // Consulta 2: facturas emitidas o recibidas por un NIT en un rango
    public ConsultaContribuyente ConsultarContribuyente(string nit, string tipo, DateTime desde, DateTime hasta)
    {
        nit = nit.Trim().ToUpper();
        bool emitidas = tipo.Trim().ToLower() == "emitidas";

        var resultado = new ConsultaContribuyente
        {
            Nit = nit,
            Tipo = emitidas ? "emitidas" : "recibidas"
        };
        var porFecha = new SortedDictionary<DateTime, ResumenDia>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;

            DateTime dia = f.Fecha.Date;
            if (dia < desde.Date || dia > hasta.Date) continue;

            string nitFactura = emitidas ? f.NitEmisor : f.NitReceptor;
            if (nitFactura != nit) continue;

            if (!porFecha.ContainsKey(dia))
            {
                porFecha[dia] = new ResumenDia
                {
                    Fecha = dia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                };
            }

            porFecha[dia].CantidadFacturas++;
            porFecha[dia].MontoTotal += f.Total;
            resultado.TotalRango += f.Total;
            resultado.Facturas.Add(f);
        }

        resultado.Dias = porFecha.Values.ToList();
        return resultado;
    }
    // Consulta 3: IVA emitido y recibido por cada NIT en UNA fecha
    public ResumenIva ResumenIvaPorFecha(DateTime fecha)
    {
        var resultado = new ResumenIva
        {
            Fecha = fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };

        // SortedDictionary: los NIT salen ordenados
        var porNit = new SortedDictionary<string, IvaPorNit>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;
            if (f.Fecha.Date != fecha.Date) continue;

            // IVA emitido para el emisor
            if (!porNit.ContainsKey(f.NitEmisor))
                porNit[f.NitEmisor] = new IvaPorNit { Nit = f.NitEmisor };
            porNit[f.NitEmisor].IvaEmitido += f.Iva;

            // IVA recibido para el receptor
            if (!porNit.ContainsKey(f.NitReceptor))
                porNit[f.NitReceptor] = new IvaPorNit { Nit = f.NitReceptor };
            porNit[f.NitReceptor].IvaRecibido += f.Iva;
        }

        resultado.Nits = porNit.Values.ToList();
        return resultado;
    }
    // Consulta 4: valor autorizado por fecha (total o sin IVA) en un rango
    public ResumenRango ResumenPorRango(DateTime desde, DateTime hasta, string tipo)
    {
        bool sinIva = tipo.Trim().ToLower() == "siniva";

        var resultado = new ResumenRango { Tipo = sinIva ? "sinIva" : "total" };
        var porFecha = new SortedDictionary<DateTime, ValorDia>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;

            DateTime dia = f.Fecha.Date;
            if (dia < desde.Date || dia > hasta.Date) continue;

            if (!porFecha.ContainsKey(dia))
            {
                porFecha[dia] = new ValorDia
                {
                    Fecha = dia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                };
            }

            decimal monto = sinIva ? f.Valor : f.Total;
            porFecha[dia].Valor += monto;
            resultado.TotalRango += monto;
        }

        resultado.Dias = porFecha.Values.ToList();
        return resultado;
    }
}