namespace Backend.Estructuras;

// TDA propio: tabla hash con encadenamiento.
// La llave es un texto (por ejemplo, un NIT). El arreglo tiene "casillas";
// cada casilla guarda una cadena de nodos con las llaves que caen en ella.
public class TablaHash<TValor>
{
    private class Nodo
    {
        public string Llave;
        public TValor Valor;
        public Nodo? Siguiente;

        public Nodo(string llave, TValor valor)
        {
            Llave = llave;
            Valor = valor;
        }
    }

    private Nodo?[] _casillas;

    public int Cantidad { get; private set; }

    public TablaHash(int capacidadInicial = 64)
    {
        _casillas = new Nodo?[capacidadInicial];
    }

    // Función hash propia: polinomio con base 31 sobre los caracteres de la llave
    private static int Indice(string llave, int tamano)
    {
        int hash = 0;
        foreach (char c in llave)
        {
            hash = unchecked(hash * 31 + c);
        }
        return (hash & 0x7FFFFFFF) % tamano;
    }

    public bool Contiene(string llave)
    {
        return Buscar(llave) != null;
    }

    public TValor? Obtener(string llave)
    {
        Nodo? nodo = Buscar(llave);
        return nodo == null ? default : nodo.Valor;
    }

    // Inserta o reemplaza. Devuelve true si la llave era nueva.
    public bool Guardar(string llave, TValor valor)
    {
        Nodo? existente = Buscar(llave);
        if (existente != null)
        {
            existente.Valor = valor;
            return false;
        }

        // Si la tabla está llena al 75 %, se duplica para que las cadenas sigan cortas
        if (Cantidad >= _casillas.Length * 3 / 4)
        {
            Crecer();
        }

        int i = Indice(llave, _casillas.Length);
        var nuevo = new Nodo(llave, valor);
        nuevo.Siguiente = _casillas[i];
        _casillas[i] = nuevo;
        Cantidad++;
        return true;
    }

    public void Limpiar()
    {
        _casillas = new Nodo?[_casillas.Length];
        Cantidad = 0;
    }

    // Devuelve todos los valores guardados en una lista enlazada propia
    public ListaEnlazada<TValor> Valores()
    {
        var lista = new ListaEnlazada<TValor>();
        foreach (Nodo? casilla in _casillas)
        {
            Nodo? actual = casilla;
            while (actual != null)
            {
                lista.Agregar(actual.Valor);
                actual = actual.Siguiente;
            }
        }
        return lista;
    }

    private Nodo? Buscar(string llave)
    {
        Nodo? actual = _casillas[Indice(llave, _casillas.Length)];
        while (actual != null)
        {
            if (actual.Llave == llave) return actual;
            actual = actual.Siguiente;
        }
        return null;
    }

    // Duplica el tamaño del arreglo y reubica cada nodo en su nueva casilla
    private void Crecer()
    {
        Nodo?[] anteriores = _casillas;
        _casillas = new Nodo?[anteriores.Length * 2];

        foreach (Nodo? casilla in anteriores)
        {
            Nodo? actual = casilla;
            while (actual != null)
            {
                Nodo? siguiente = actual.Siguiente;
                int i = Indice(actual.Llave, _casillas.Length);
                actual.Siguiente = _casillas[i];
                _casillas[i] = actual;
                actual = siguiente;
            }
        }
    }
}
