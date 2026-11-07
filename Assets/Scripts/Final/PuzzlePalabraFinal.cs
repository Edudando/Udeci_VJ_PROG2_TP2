using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GrabadoPiedra))]
public sealed class PuzzlePalabraFinal : MonoBehaviour
{
    [Tooltip("Asigná las seis bases de izquierda a derecha, mirándolas desde Nia.")]
    [SerializeField] private BaseDadoFinal[] basesEnOrden = new BaseDadoFinal[6];

    private GrabadoPiedra grabado;
    private bool resuelto;

    private void Awake()
    {
        grabado = GetComponent<GrabadoPiedra>();
        if (ReferenciasCompletas(basesEnOrden)) return;

        BaseDadoFinal[] candidatas = GetComponentsInChildren<BaseDadoFinal>(true);
        BaseDadoFinal[] basesPorNombre = new BaseDadoFinal[6];
        for (int i = 0; i < basesPorNombre.Length; i++)
        {
            string nombre = $"Base_{i + 1:00}";
            basesPorNombre[i] = System.Array.Find(candidatas, candidata => candidata.name == nombre);
        }

        if (ReferenciasCompletas(basesPorNombre))
            basesEnOrden = basesPorNombre;
        else
            Debug.LogWarning("[NIA] Asigná las seis bases de izquierda a derecha en Bases En Orden de stone 1.", this);
    }

    public void ActualizarSolucion(string palabra)
    {
        string palabraEnBases = LeerPalabra(palabra);
        bool coincide = palabraEnBases != null && palabraEnBases == palabra;
        if (palabraEnBases != null && !coincide)
            Debug.Log($"[NIA] Orden leído en las bases: {palabraEnBases} | Objetivo: {palabra}", this);

        if (coincide == resuelto) return;
        resuelto = coincide;
        grabado.MostrarColor(resuelto);
        if (resuelto)
            Debug.Log($"[NIA] Palabra correcta: {palabra}", this);
    }

    private string LeerPalabra(string palabra)
    {
        if (string.IsNullOrEmpty(palabra) || palabra.Length != 6 ||
            !ReferenciasCompletas(basesEnOrden)) return null;

        char[] letras = new char[6];
        for (int i = 0; i < basesEnOrden.Length; i++)
        {
            letras[i] = basesEnOrden[i].ObtenerLetra(palabra);
            if (letras[i] == '\0') return null;
        }
        return new string(letras);
    }

    private static bool ReferenciasCompletas(BaseDadoFinal[] bases)
    {
        if (bases == null || bases.Length != 6) return false;

        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i] == null) return false;
            for (int anterior = 0; anterior < i; anterior++)
                if (bases[anterior] == bases[i]) return false;
        }
        return true;
    }
}
