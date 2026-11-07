/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Guarda las caras recogidas y las muestra en la barra inferior.
 *********************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventarioLetras : MonoBehaviour
{
    [Tooltip("Arrastrar Cara_1 a Cara_6, en ese orden.")]
    [SerializeField] private Image[] casilleros = new Image[6];
    private readonly List<Texture2D> caras = new List<Texture2D>();
    private readonly List<int> indicesCaras = new List<int>();
    private readonly Dictionary<Texture2D, Sprite> sprites = new Dictionary<Texture2D, Sprite>();

    private int[] ordenCaras;
    private string palabraObjetivo;
    private int dadosRecogidos;

    public int Cantidad => caras.Count;
    public bool EstaLleno => casilleros == null || caras.Count >= casilleros.Length;
    public IReadOnlyList<Texture2D> Caras => caras;
    public IReadOnlyList<int> IndicesCaras => indicesCaras;
    public string PalabraObjetivo => palabraObjetivo;

    private void Awake()
    {
        if (casilleros == null) return;
        foreach (Image casillero in casilleros)
        {
            if (casillero == null) continue;
            casillero.sprite = null;
            casillero.enabled = false;
            casillero.raycastTarget = false;
        }
    }

    public bool RecogerDado(DadoCara dado)
    {
        if (dado == null || EstaLleno) return false;
        if (casilleros.Length != 6)
        {
            Debug.LogError("InventarioLetras necesita exactamente seis casilleros.", this);
            return false;
        }
        ControladorPalabras controlador = dado.GetComponent<ControladorPalabras>();
        string palabra = controlador != null ? controlador.PalabraActual : null;
        if (string.IsNullOrEmpty(palabra) || palabra.Length != 6)
        {
            Debug.LogError("El dado todavía no recibió una palabra de seis letras.", dado);
            return false;
        }
        for (int i = 0; i < 6; i++)
            if (dado.ObtenerTexturaCara(i) == null)
            {
                Debug.LogError("El dado no tiene las seis texturas asignadas.", dado);
                return false;
            }
        if (ordenCaras == null)
        {
            int[] propuesta = new int[6];
            if (!BuscarOrden(palabra, 0, propuesta, new bool[6]))
            {
                Debug.LogError("No se puede desordenar " + palabra +
                    " sin dejar letras en su posición correcta. Revisar palabras con más de tres letras iguales.", this);
                return false;
            }
            ordenCaras = propuesta;
            palabraObjetivo = palabra;
        }
        if (palabra != palabraObjetivo)
        {
            Debug.LogError("Todos los dados deben tener la misma palabra que inició este inventario.", dado);
            return false;
        }
        if (dadosRecogidos >= ordenCaras.Length) return false;
        int indice = ordenCaras[dadosRecogidos];
        if (!AgregarCara(dado.ObtenerTexturaCara(indice), indice)) return false;

        dadosRecogidos++;
        Debug.Log("Letra recogida: " + caras[Cantidad - 1].name +
            ". Inventario: " + Cantidad + "/" + casilleros.Length, this);
        return true;
    }

    public bool RetirarPrimera()
    {
        if (Cantidad == 0) return false;

        caras.RemoveAt(0);
        indicesCaras.RemoveAt(0);
        for (int i = 0; i < Cantidad; i++)
            MostrarCara(casilleros[i], sprites[caras[i]]);

        Image ultimoOcupado = casilleros[Cantidad];
        ultimoOcupado.sprite = null;
        ultimoOcupado.enabled = false;
        return true;
    }

    public bool DevolverAlFinal(Texture2D textura, int indiceCara)
    {
        if (indiceCara < 0 || indiceCara >= 6) return false;
        return AgregarCara(textura, indiceCara);
    }

    // Prueba alternativas hasta completar los seis lugares: así no queda
    // una última letra que solo pueda ir en su posición correcta.
    private bool BuscarOrden(string palabra, int posicion, int[] orden, bool[] usadas)
    {
        if (posicion == 6) return true;
        int[] candidatas = { 0, 1, 2, 3, 4, 5 };
        for (int i = candidatas.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temporal = candidatas[i];
            candidatas[i] = candidatas[j];
            candidatas[j] = temporal;
        }
        foreach (int cara in candidatas)
        {
            // Compara letras, no solo índices: contempla las repetidas.
            if (usadas[cara] || palabra[cara] == palabra[posicion]) continue;
            usadas[cara] = true;
            orden[posicion] = cara;
            if (BuscarOrden(palabra, posicion + 1, orden, usadas)) return true;
            usadas[cara] = false;
        }
        return false;
    }

    private bool AgregarCara(Texture2D textura, int indiceCara)
    {
        if (textura == null || EstaLleno) return false;
        Image destino = casilleros[caras.Count];
        if (destino == null)
        {
            Debug.LogError("InventarioLetras: falta asignar el casillero " + (caras.Count + 1), this);
            return false;
        }
        if (!sprites.TryGetValue(textura, out Sprite sprite))
        {
            sprite = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = textura.name;
            sprites.Add(textura, sprite);
        }
        caras.Add(textura);
        indicesCaras.Add(indiceCara);
        MostrarCara(destino, sprite);
        return true;
    }

    private static void MostrarCara(Image destino, Sprite sprite)
    {
        destino.sprite = sprite;
        destino.color = Color.white;
        destino.preserveAspect = true;
        destino.enabled = true;
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in sprites.Values)
            if (sprite != null) Destroy(sprite);
    }
}
