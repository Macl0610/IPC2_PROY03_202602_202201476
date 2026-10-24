using System.Text.Json;
using Backend.Estructuras;
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
    // TDA propio: lista enlazada con todas las facturas recibidas
    private readonly ListaEnlazada<Factura> _facturas;
    private readonly RtuService _rtu;
    private readonly object _lock = new();

    public FacturaService(RtuService rtu)
    {
        _rtu = rtu;
        _facturas = Cargar();
    }

    public RespuestaAutorizacion Autorizar(ListaEnlazada<SolicitudFactura> solicitudes)
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
                    resultado.DetalleAprobaciones.Agregar(new DetalleAprobacion
                    {
                        Referencia = factura.Referencia,
                        CodigoAprobacion = factura.CodigoAprobacion.Value
                    });
                }
                else
                {
                    if (factura.Errores.Contiene(NitEmisorInexistente))
                        resultado.RechazadasEmisorInexistente++;
                    if (factura.Errores.Contiene(NitReceptorInexistente))
                        resultado.RechazadasReceptorInexistente++;
                }

                _facturas.Agregar(factura);
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
            f.Errores.Agregar(FechaInvalida);
            f.Fecha = DateTime.Now;
        }
        else
        {
            f.Fecha = fecha.Value;
        }

        // Referencia: obligatoria, máximo 40, única
        if (f.Referencia.Length == 0 || f.Referencia.Length > 40)
            f.Errores.Agregar(ReferenciaInvalida);
        else if (ExisteReferencia(f.Referencia))
            f.Errores.Agregar(ReferenciaDuplicada);

        // NIT emisor
        if (!ValidadorNit.EsValido(f.NitEmisor))
            f.Errores.Agregar(NitEmisorInvalido);
        else if (!_rtu.Existe(f.NitEmisor))
            f.Errores.Agregar(NitEmisorInexistente);

        // NIT receptor
        if (!ValidadorNit.EsValido(f.NitReceptor))
            f.Errores.Agregar(NitReceptorInvalido);
        else if (!_rtu.Existe(f.NitReceptor))
            f.Errores.Agregar(NitReceptorInexistente);

        // VALOR: positivo y con máximo 2 decimales
        // (si al redondearlo a 2 decimales cambia, es que traía más)
        if (f.Valor <= 0 || f.Valor != Math.Round(f.Valor, 2))
            f.Errores.Agregar(ValorInvalido);

        decimal ivaCorrecto = Math.Round(f.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
        if (f.Iva != ivaCorrecto)
            f.Errores.Agregar(IvaMalCalculado);

        if (f.Total != f.Valor + f.Iva)
            f.Errores.Agregar(TotalMalCalculado);

        return f;
    }

    // La referencia debe ser única entre TODAS las facturas recibidas
    private bool ExisteReferencia(string referencia)
    {
        foreach (var f in _facturas)
        {
            if (f.Referencia == referencia) return true;
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

    public ListaEnlazada<Factura> ObtenerTodas()
    {
        lock (_lock)
        {
            return _facturas.Copiar();
        }
    }

    public void Limpiar()
    {
        lock (_lock)
        {
            _facturas.Limpiar();
            Guardar();
        }
    }

    private ListaEnlazada<Factura> Cargar()
    {
        if (!File.Exists(_ruta)) return new ListaEnlazada<Factura>();

        string json = File.ReadAllText(_ruta);
        return JsonSerializer.Deserialize<ListaEnlazada<Factura>>(json) ?? new ListaEnlazada<Factura>();
    }

    private void Guardar()
    {
        Directory.CreateDirectory(_carpeta);
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_ruta, JsonSerializer.Serialize(_facturas, opciones));
    }
}