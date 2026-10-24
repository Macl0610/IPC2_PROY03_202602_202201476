using System.Collections;

namespace Backend.Estructuras;

// TDA propio: lista simplemente enlazada.
// Cada nodo guarda un dato y la referencia al nodo siguiente.
// Implementa ICollection<T> solo para que el convertidor de JSON sepa
// leerla y escribirla como un arreglo [ ... ].
public class ListaEnlazada<T> : ICollection<T>
{
    private class Nodo
    {
        public T Dato;
        public Nodo? Siguiente;

        public Nodo(T dato)
        {
            Dato = dato;
        }
    }

    private Nodo? _primero;
    private Nodo? _ultimo;

    public int Cantidad { get; private set; }

    // Agrega al final. Como guardamos el último nodo, no hay que recorrer la lista.
    public void Agregar(T dato)
    {
        var nuevo = new Nodo(dato);

        if (_primero == null)
        {
            _primero = nuevo;
            _ultimo = nuevo;
        }
        else
        {
            _ultimo!.Siguiente = nuevo;
            _ultimo = nuevo;
        }

        Cantidad++;
    }

    // Recorre los nodos buscando un dato igual
    public bool Contiene(T dato)
    {
        Nodo? actual = _primero;
        while (actual != null)
        {
            if (Equals(actual.Dato, dato)) return true;
            actual = actual.Siguiente;
        }
        return false;
    }

    // Quita el primer nodo que tenga ese dato, reconectando el anterior con el siguiente
    public bool Eliminar(T dato)
    {
        Nodo? anterior = null;
        Nodo? actual = _primero;

        while (actual != null)
        {
            if (Equals(actual.Dato, dato))
            {
                if (anterior == null)
                    _primero = actual.Siguiente;
                else
                    anterior.Siguiente = actual.Siguiente;

                if (actual == _ultimo) _ultimo = anterior;

                Cantidad--;
                return true;
            }
            anterior = actual;
            actual = actual.Siguiente;
        }
        return false;
    }

    public void Limpiar()
    {
        _primero = null;
        _ultimo = null;
        Cantidad = 0;
    }

    // Devuelve una lista nueva con los mismos datos
    public ListaEnlazada<T> Copiar()
    {
        var copia = new ListaEnlazada<T>();
        Nodo? actual = _primero;
        while (actual != null)
        {
            copia.Agregar(actual.Dato);
            actual = actual.Siguiente;
        }
        return copia;
    }

    // Permite recorrer la lista con foreach
    public IEnumerator<T> GetEnumerator()
    {
        Nodo? actual = _primero;
        while (actual != null)
        {
            yield return actual.Dato;
            actual = actual.Siguiente;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ---- Lo que pide ICollection<T>, conectado a nuestros propios métodos ----
    int ICollection<T>.Count => Cantidad;
    bool ICollection<T>.IsReadOnly => false;
    void ICollection<T>.Add(T item) => Agregar(item);
    void ICollection<T>.Clear() => Limpiar();
    bool ICollection<T>.Contains(T item) => Contiene(item);
    bool ICollection<T>.Remove(T item) => Eliminar(item);

    void ICollection<T>.CopyTo(T[] arreglo, int inicio)
    {
        Nodo? actual = _primero;
        int i = inicio;
        while (actual != null)
        {
            arreglo[i] = actual.Dato;
            i++;
            actual = actual.Siguiente;
        }
    }
}
