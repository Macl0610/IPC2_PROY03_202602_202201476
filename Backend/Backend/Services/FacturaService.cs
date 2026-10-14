using System.Text.Json;
using Backend.Models;
using Backend.Utils;

namespace Backend.Services;

public class FacturaService
{
    // Tipos de error (los usaremos en los reportes)
    public const string FechaInvalida = "FECHA_INVALIDA";
    public const string ReferenciaInvalida = "REFERENCIA_INVALIDA";
    public const string ReferenciaDuplicada = "REFERENCIA_DUPLICADA";
    public const string NitEmisorInvalido = "NIT_EMISOR_INVALIDO";
    public const string NitEmisorInexistente = "NIT_EMISOR_INEXISTENTE";
    public const string NitReceptorInvalido = "NIT_RECEPTOR_INVALIDO";
    public const string NitReceptorInexistente = "NIT_RECEPTOR_INEXISTENTE";
    public const string ValorInvalido = "VALOR_INVALIDO";
    public const string IvaMalCalculado = "IVA_MAL_CALCULADO";
    public const string TotalMalCalculado = "TOTAL_MAL_CALCULADO";

    private readonly string _carpeta = "Data";
    private readonly string _ruta = Path.Combine("Data", "facturas.json");
    private readonly List<Factura> _facturas;
    private readonly RtuService _rtu;
    private readonly object _lock = new();

    public FacturaService(RtuService rtu)
    {
        _rtu = rtu;
        _facturas = Cargar();
    }

    public RespuestaAutorizacion Autorizar(List<SolicitudFactura> solicitudes)
    {
        var respuesta = new RespuestaAutorizacion();
        var resultado = respuesta.ResultadoAutorizaciones;

        lock (_lock)
        {
            foreach (var s in solicitudes)
            {
                Factura factura = Validar(s);

                if (factura.Aprobada)
                {
                    factura.CodigoAprobacion = SiguienteCodigo(DateTime.Now);
                    resultado.Aprobadas++;
                    resultado.DetalleAprobaciones.Add(new DetalleAprobacion
                    {
                        Referencia = factura.Referencia,
                        CodigoAprobacion = factura.CodigoAprobacion.Value
                    });
                }
                else
                {
                    if (factura.Errores.Contains(NitEmisorInexistente))
                        resultado.RechazadasEmisorInexistente++;
                    if (factura.Errores.Contains(NitReceptorInexistente))
                        resultado.RechazadasReceptorInexistente++;
                }

                _facturas.Add(factura);
            }

            Guardar();
        }

        return respuesta;
    }

    private Factura Validar(SolicitudFactura s)
    {
        var f = new Factura
        {
            Referencia = (s.Referencia ?? "").Trim(),
            NitEmisor = (s.NitEmisor ?? "").Trim().ToUpper(),
            NitReceptor = (s.NitReceptor ?? "").Trim().ToUpper(),
            Valor = s.Valor,
            Iva = s.Iva,
            Total = s.Total
        };

        // Fecha (con expresión regular)
        DateTime? fecha = ExtractorFecha.Extraer(s.Tiempo);
        if (fecha == null)
        {
            f.Errores.Add(FechaInvalida);
            f.Fecha = DateTime.Now;
        }
        else
        {
            f.Fecha = fecha.Value;
        }

        // Referencia: obligatoria, máximo 40, única
        if (f.Referencia.Length == 0 || f.Referencia.Length > 40)
            f.Errores.Add(ReferenciaInvalida);
        else if (ExisteReferenciaAprobada(f.Referencia))
            f.Errores.Add(ReferenciaDuplicada);

        // NIT emisor
        if (!ValidadorNit.EsValido(f.NitEmisor))
            f.Errores.Add(NitEmisorInvalido);
        else if (!_rtu.Existe(f.NitEmisor))
            f.Errores.Add(NitEmisorInexistente);

        // NIT receptor
        if (!ValidadorNit.EsValido(f.NitReceptor))
            f.Errores.Add(NitReceptorInvalido);
        else if (!_rtu.Existe(f.NitReceptor))
            f.Errores.Add(NitReceptorInexistente);

        // Montos
        if (f.Valor <= 0)
            f.Errores.Add(ValorInvalido);

        decimal ivaCorrecto = Math.Round(f.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
        if (f.Iva != ivaCorrecto)
            f.Errores.Add(IvaMalCalculado);

        if (f.Total != f.Valor + ivaCorrecto)
            f.Errores.Add(TotalMalCalculado);

        return f;
    }

    private bool ExisteReferenciaAprobada(string referencia)
    {
        foreach (var f in _facturas)
        {
            if (f.Aprobada && f.Referencia == referencia) return true;
        }
        return false;
    }

    // yyyymmdd + correlativo de 8 dígitos que reinicia cada día
    private long SiguienteCodigo(DateTime hoy)
    {
        long prefijo = long.Parse(hoy.ToString("yyyyMMdd"));
        long ultimoCorrelativo = 0;

        foreach (var f in _facturas)
        {
            if (f.CodigoAprobacion.HasValue && f.CodigoAprobacion.Value / 100000000 == prefijo)
            {
                long correlativo = f.CodigoAprobacion.Value % 100000000;
                if (correlativo > ultimoCorrelativo) ultimoCorrelativo = correlativo;
            }
        }

        return prefijo * 100000000 + (ultimoCorrelativo + 1);
    }

    public List<Factura> ObtenerTodas()
    {
        lock (_lock)
        {
            return _facturas.ToList();
        }
    }

    public void Limpiar()
    {
        lock (_lock)
        {
            _facturas.Clear();
            Guardar();
        }
    }

    private List<Factura> Cargar()
    {
        if (!File.Exists(_ruta)) return new List<Factura>();
        string json = File.ReadAllText(_ruta);
        return JsonSerializer.Deserialize<List<Factura>>(json) ?? new List<Factura>();
    }

    private void Guardar()
    {
        Directory.CreateDirectory(_carpeta);
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_ruta, JsonSerializer.Serialize(_facturas, opciones));
    }
}