using System.Text.Json;
using Backend.Models;
using Backend.Utils;

namespace Backend.Services;

public class RtuService
{
    private readonly string _carpeta = "Data";
    private readonly string _ruta = Path.Combine("Data", "rtu.json");
    private readonly Dictionary<string, Contribuyente> _contribuyentes;
    private readonly object _lock = new();

    public RtuService()
    {
        _contribuyentes = Cargar();
    }

    public RespuestaRtu Actualizar(List<Contribuyente> lista)
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

                if (_contribuyentes.ContainsKey(nit))
                    respuesta.ContribuyentesActualizados++;
                else
                    respuesta.ContribuyentesNuevos++;

                _contribuyentes[nit] = new Contribuyente
                {
                    Nit = nit,
                    Nombre = (c.Nombre ?? "").Trim()
                };
            }

            Guardar();
        }

        return respuesta;
    }

    public bool Existe(string nit)
    {
        lock (_lock)
        {
            return _contribuyentes.ContainsKey(nit.Trim().ToUpper());
        }
    }

    public List<Contribuyente> ObtenerTodos()
    {
        lock (_lock)
        {
            return _contribuyentes.Values.ToList();
        }
    }

    public void Limpiar()
    {
        lock (_lock)
        {
            _contribuyentes.Clear();
            Guardar();
        }
    }

    private Dictionary<string, Contribuyente> Cargar()
    {
        if (!File.Exists(_ruta)) return new Dictionary<string, Contribuyente>();

        string json = File.ReadAllText(_ruta);
        var lista = JsonSerializer.Deserialize<List<Contribuyente>>(json) ?? new List<Contribuyente>();
        return lista.ToDictionary(c => c.Nit);
    }

    private void Guardar()
    {
        Directory.CreateDirectory(_carpeta);
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_ruta, JsonSerializer.Serialize(_contribuyentes.Values, opciones));
    }
}