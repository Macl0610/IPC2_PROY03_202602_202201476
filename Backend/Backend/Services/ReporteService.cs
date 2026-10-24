using System.Globalization;
using Backend.Estructuras;
using Backend.Models;

namespace Backend.Services;

public class ReporteService
{
    private readonly FacturaService _facturas;

    public ReporteService(FacturaService facturas)
    {
        _facturas = facturas;
    }

    // Acumula lo de UN día del reporte por fechas
    private class AcumuladoDia
    {
        public ReporteFecha Reporte = new();
        public TablaHash<bool> Emisores = new(); // NIT distintos (la tabla no repite llaves)
        public TablaHash<bool> Receptores = new();
    }

    private static string Formato(DateTime dia)
    {
        return dia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    // Consulta 1: reporte de cada fecha dentro del rango [desde, hasta]
    public ListaEnlazada<ReporteFecha> ReportePorFechas(DateTime desde, DateTime hasta)
    {
        // TDA propio: lista ordenada por fecha
        var porFecha = new ListaOrdenada<DateTime, AcumuladoDia>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            DateTime dia = f.Fecha.Date; // la fecha sin la hora
            if (dia < desde.Date || dia > hasta.Date) continue;

            AcumuladoDia a = porFecha.ObtenerOCrear(dia, () => new AcumuladoDia
            {
                Reporte = new ReporteFecha { Fecha = Formato(dia) }
            });
            ReporteFecha r = a.Reporte;
            r.TotalFacturas++;

            if (f.Errores.Contiene(FacturaService.NitEmisorInvalido)) r.NitEmisorInvalido++;
            if (f.Errores.Contiene(FacturaService.NitReceptorInvalido)) r.NitReceptorInvalido++;
            if (f.Errores.Contiene(FacturaService.NitEmisorInexistente)) r.NitEmisorInexistente++;
            if (f.Errores.Contiene(FacturaService.NitReceptorInexistente)) r.NitReceptorInexistente++;
            if (f.Errores.Contiene(FacturaService.IvaMalCalculado)) r.IvaMalCalculado++;
            if (f.Errores.Contiene(FacturaService.TotalMalCalculado)) r.TotalMalCalculado++;
            if (f.Errores.Contiene(FacturaService.ReferenciaDuplicada)) r.ReferenciaDuplicada++;

            if (f.Aprobada)
            {
                r.FacturasSinErrores++;
                a.Emisores.Guardar(f.NitEmisor, true);
                a.Receptores.Guardar(f.NitReceptor, true);
            }
        }

        var resultado = new ListaEnlazada<ReporteFecha>();
        foreach (AcumuladoDia a in porFecha.Valores())
        {
            a.Reporte.CantidadEmisores = a.Emisores.Cantidad;
            a.Reporte.CantidadReceptores = a.Receptores.Cantidad;
            resultado.Agregar(a.Reporte);
        }
        return resultado;
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
        var porFecha = new ListaOrdenada<DateTime, ResumenDia>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;

            DateTime dia = f.Fecha.Date;
            if (dia < desde.Date || dia > hasta.Date) continue;

            string nitFactura = emitidas ? f.NitEmisor : f.NitReceptor;
            if (nitFactura != nit) continue;

            ResumenDia r = porFecha.ObtenerOCrear(dia, () => new ResumenDia { Fecha = Formato(dia) });
            r.CantidadFacturas++;
            r.MontoTotal += f.Total;
            resultado.TotalRango += f.Total;
            resultado.Facturas.Agregar(f);
        }

        resultado.Dias = porFecha.Valores();
        return resultado;
    }

    // Consulta 3: IVA emitido y recibido por cada NIT en UNA fecha
    public ResumenIva ResumenIvaPorFecha(DateTime fecha)
    {
        var resultado = new ResumenIva { Fecha = Formato(fecha) };

        // TDA propio: lista ordenada por NIT
        var porNit = new ListaOrdenada<string, IvaPorNit>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;
            if (f.Fecha.Date != fecha.Date) continue;

            // IVA emitido para el emisor
            IvaPorNit emisor = porNit.ObtenerOCrear(f.NitEmisor, () => new IvaPorNit { Nit = f.NitEmisor });
            emisor.IvaEmitido += f.Iva;

            // IVA recibido para el receptor
            IvaPorNit receptor = porNit.ObtenerOCrear(f.NitReceptor, () => new IvaPorNit { Nit = f.NitReceptor });
            receptor.IvaRecibido += f.Iva;
        }

        resultado.Nits = porNit.Valores();
        return resultado;
    }

    // Consulta 4: valor autorizado por fecha (total o sin IVA) en un rango
    public ResumenRango ResumenPorRango(DateTime desde, DateTime hasta, string tipo)
    {
        bool sinIva = tipo.Trim().ToLower() == "siniva";

        var resultado = new ResumenRango { Tipo = sinIva ? "sinIva" : "total" };
        var porFecha = new ListaOrdenada<DateTime, ValorDia>();

        foreach (var f in _facturas.ObtenerTodas())
        {
            if (!f.Aprobada) continue;

            DateTime dia = f.Fecha.Date;
            if (dia < desde.Date || dia > hasta.Date) continue;

            ValorDia v = porFecha.ObtenerOCrear(dia, () => new ValorDia { Fecha = Formato(dia) });
            decimal monto = sinIva ? f.Valor : f.Total;
            v.Valor += monto;
            resultado.TotalRango += monto;
        }

        resultado.Dias = porFecha.Valores();
        return resultado;
    }
}
