using System.Text.Json;
using Backend.Estructuras;
using Backend.Models;
using Backend.Utils;

namespace Backend.Services;

public class RtuService
{
    private readonly string _carpeta = "Data";
    private readonly string _ruta = Path.Combine("Data", "rtu.json");

    // TDA propio: tabla hash donde la llave es el NIT
    private readonly TablaHash<Contribuyente> _contribuyentes;
    private readonly object _lock = new();

    public RtuService()
    {
        _contribuyentes = Cargar();
    }

    public RespuestaRtu Actualizar(ListaEnlazada<Contribuyente> lista)
    {
        var respuesta = new RespuestaRtu();

        lock (_lock)
        {
            foreach (var c in lista)
            {
                string nit = (c.Nit ?? "").Trim().ToUpper();

                // NIT inválido: no tiene efecto sobre el RTU
                if (!ValidadorNit.EsValido(nit))
                {
                    respuesta.NitsInvalidos++;
                    continue;
                }

                var contribuyente = new Contribuyente
                {
                    Nit = nit,
                    Nombre = (c.Nombre ?? "").Trim()
                };

                // Guardar devuelve true si el NIT era nuevo
                if (_contribuyentes.Guardar(nit, contribuyente))
                    respuesta.ContribuyentesNuevos++;
                else
                    respuesta.ContribuyentesActualizados++;
            }

            Guardar();
        }

        return respuesta;
    }

    public bool Existe(string nit)
    {
        lock (_lock)
        {
            return _contribuyentes.Contiene(nit.Trim().ToUpper());
        }
    }

    public ListaEnlazada<Contribuyente> ObtenerTodos()
    {
        lock (_lock)
        {
            return _contribuyentes.Valores();
        }
    }

    public void Limpiar()
    {
        lock (_lock)
        {
            _contribuyentes.Limpiar();
            Guardar();
        }
    }

    private TablaHash<Contribuyente> Cargar()
    {
        var tabla = new TablaHash<Contribuyente>();
        if (!File.Exists(_ruta)) return tabla;

        string json = File.ReadAllText(_ruta);
        var lista = JsonSerializer.Deserialize<ListaEnlazada<Contribuyente>>(json) ?? new ListaEnlazada<Contribuyente>();
        foreach (var c in lista)
        {
            tabla.Guardar(c.Nit, c);
        }
        return tabla;
    }

    private void Guardar()
    {
        Directory.CreateDirectory(_carpeta);
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_ruta, JsonSerializer.Serialize(_contribuyentes.Valores(), opciones));
    }
}
