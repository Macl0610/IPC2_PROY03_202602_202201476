namespace Backend.Estructuras;

// TDA propio: lista enlazada que mantiene sus elementos ordenados por llave.
// Se usa en los reportes para agrupar por fecha o por NIT y que salgan en orden.
public class ListaOrdenada<TClave, TValor> where TClave : IComparable<TClave>
{
    private class Nodo
    {
        public TClave Llave;
        public TValor Valor;
        public Nodo? Siguiente;

        public Nodo(TClave llave, TValor valor)
        {
            Llave = llave;
            Valor = valor;
        }
    }

    private Nodo? _primero;

    public int Cantidad { get; private set; }

    // Busca la llave. Si no existe, crea el valor con "crear" y lo inserta
    // en la posición que le toca para que la lista siga ordenada.
    public TValor ObtenerOCrear(TClave llave, Func<TValor> crear)
    {
        Nodo? anterior = null;
        Nodo? actual = _primero;

        while (actual != null && actual.Llave.CompareTo(llave) < 0)
        {
            anterior = actual;
            actual = actual.Siguiente;
        }

        if (actual != null && actual.Llave.CompareTo(llave) == 0)
        {
            return actual.Valor;
        }

        var nuevo = new Nodo(llave, crear());
        nuevo.Siguiente = actual;

        if (anterior == null)
            _primero = nuevo;
        else
            anterior.Siguiente = nuevo;

        Cantidad++;
        return nuevo.Valor;
    }

    // Devuelve los valores en orden de llave
    public ListaEnlazada<TValor> Valores()
    {
        var lista = new ListaEnlazada<TValor>();
        Nodo? actual = _primero;
        while (actual != null)
        {
            lista.Agregar(actual.Valor);
            actual = actual.Siguiente;
        }
        return lista;
    }
}
