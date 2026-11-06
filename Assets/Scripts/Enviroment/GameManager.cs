using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referencias de la escena")]
    [SerializeField] private ControladorPalabras controladorPalabras;
    [Tooltip("Arrastrar los seis dados de letras de la escena, uno por elemento.")]
    [SerializeField] private ControladorPalabras[] dadosDeLetras = new ControladorPalabras[6];
    [Header("Pista grabada")]
    [SerializeField] private GrabadoPiedra grabadoPiedra;

    [Header("Dificultad actual")]
    public int dificultadSeleccionada = 1;
    private ListaPalabras datosJuego;

    private void Start()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("palabras");
        if (jsonText == null)
        {
            Debug.LogError("Coloca palabras.json en Assets/Resources/palabras.json", this);
            return;
        }
        try { datosJuego = JsonUtility.FromJson<ListaPalabras>(jsonText.text); }
        catch (System.ArgumentException error)
        {
            Debug.LogError("No se pudo leer palabras.json: " + error.Message, this);
            return;
        }
        IniciarNuevaPalabra(dificultadSeleccionada);
    }

    public void IniciarNuevaPalabra(int dificultad)
    {
        if (datosJuego == null || datosJuego.palabras == null)
        {
            Debug.LogError("No hay una lista de palabras válida.", this);
            return;
        }
        List<ItemPalabra> candidatas = datosJuego.palabras.FindAll(item =>
            item != null && item.dificultad == dificultad &&
            !string.IsNullOrWhiteSpace(item.palabra) && item.palabra.Trim().Length == 6);
        if (candidatas.Count == 0)
        {
            Debug.LogWarning("No hay palabras de 6 letras para la dificultad " + dificultad, this);
            return;
        }
        ItemPalabra elegida = candidatas[Random.Range(0, candidatas.Count)];
        ArmarPalabraEnDados(elegida.palabra, elegida.imagen);
    }

    // Se conserva el nombre para las referencias existentes.
    public void ArmarPalabraEnDados(string palabra)
    {
        ItemPalabra item = datosJuego?.palabras?.Find(candidata =>
            candidata != null && string.Equals(candidata.palabra?.Trim(), palabra?.Trim(),
                System.StringComparison.OrdinalIgnoreCase));
        ArmarPalabraEnDados(palabra, item?.imagen);
    }

    private void ArmarPalabraEnDados(string palabra, string imagen)
    {
        if (grabadoPiedra != null)
            grabadoPiedra.MostrarImagen(imagen);

        // Sortear una sola palabra y aplicarla a todas las copias.
        // Se mantiene la referencia anterior por compatibilidad con la escena.
        HashSet<ControladorPalabras> destinos = new HashSet<ControladorPalabras>();
        if (controladorPalabras != null) destinos.Add(controladorPalabras);
        if (dadosDeLetras != null)
            foreach (ControladorPalabras dado in dadosDeLetras)
                if (dado != null) destinos.Add(dado);

        if (destinos.Count == 0)
        {
            Debug.LogError("Asigna los seis ControladorPalabras en GameManager > Dados De Letras.", this);
            return;
        }
        if (destinos.Count != 6)
            Debug.LogWarning("Se esperaban seis dados distintos y hay " + destinos.Count + " asignados.", this);
        foreach (ControladorPalabras dado in destinos)
            dado.MostrarPalabra(palabra);
    }
}
