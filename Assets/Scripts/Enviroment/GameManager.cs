using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int CantidadDados = 6;

    [Header("Referencias de la escena")]
    [SerializeField] private ControladorPalabras controladorPalabras;
    [Tooltip("Arrastrar los seis dados de letras de la escena, uno por elemento.")]
    [SerializeField] private ControladorPalabras[] dadosDeLetras = new ControladorPalabras[6];
    [Header("Pista grabada")]
    [SerializeField] private GrabadoPiedra grabadoPiedra;
    [SerializeField, Min(0.01f)] private float profundidadPiedra = 2.5f;
    [SerializeField, Min(0.01f)] private float duracionAscenso = 1.5f;

    [Header("Dificultad actual")]
    public int dificultadSeleccionada = 1;
    private ListaPalabras datosJuego;
    private readonly HashSet<int> dadosDerrotados = new HashSet<int>();

    private void Start()
    {
        if (grabadoPiedra != null && grabadoPiedra.gameObject.activeSelf)
            Debug.LogWarning("[NIA] Desactivá solo el objeto stone 1 en la escena para que empiece oculto.", grabadoPiedra);

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
        List<ItemPalabra> palabrasDeSeisLetras = datosJuego.palabras.FindAll(item =>
            item != null &&
            !string.IsNullOrWhiteSpace(item.palabra) && item.palabra.Trim().Length == CantidadDados);
        HashSet<ControladorPalabras> destinos = ObtenerDados();
        if (destinos.Count == 0) return;

        List<ItemPalabra> candidatas = palabrasDeSeisLetras.FindAll(item => item.dificultad == dificultad);
        if (IntentarSortearPalabra(candidatas, destinos)) return;

        Debug.LogWarning($"[NIA] No hay palabras utilizables de dificultad {dificultad}. Buscando en las demás dificultades.", this);
        candidatas = palabrasDeSeisLetras.FindAll(item => item.dificultad != dificultad);
        if (!IntentarSortearPalabra(candidatas, destinos))
            Debug.LogError("[NIA] Ninguna palabra de seis letras tiene sus seis texturas disponibles. Revisá Resources/Alfabeto.", this);
    }

    private bool IntentarSortearPalabra(List<ItemPalabra> candidatas, HashSet<ControladorPalabras> destinos)
    {
        while (candidatas.Count > 0)
        {
            int indice = Random.Range(0, candidatas.Count);
            ItemPalabra candidata = candidatas[indice];
            candidatas.RemoveAt(indice);

            string palabra = candidata.palabra.Trim().ToUpperInvariant();
            if (!ValidarTexturas(palabra, destinos)) continue;

            Debug.Log($"[NIA] Palabra seleccionada: {palabra} | Dificultad: {candidata.dificultad} | Dados: {destinos.Count}", this);
            MostrarPalabraEnDados(palabra, candidata.imagen, destinos);
            return true;
        }

        return false;
    }

    // Se conserva el nombre para las referencias existentes.
    public void ArmarPalabraEnDados(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra) || palabra.Trim().Length != CantidadDados)
        {
            Debug.LogError("[NIA] Se necesita una palabra de seis letras.", this);
            return;
        }

        palabra = palabra.Trim().ToUpperInvariant();
        HashSet<ControladorPalabras> destinos = ObtenerDados();
        if (destinos.Count == 0 || !ValidarTexturas(palabra, destinos)) return;

        ItemPalabra item = datosJuego?.palabras?.Find(candidata =>
            candidata != null && string.Equals(candidata.palabra?.Trim(), palabra,
                System.StringComparison.OrdinalIgnoreCase));
        MostrarPalabraEnDados(palabra, item?.imagen, destinos);
    }

    private static bool ValidarTexturas(string palabra, HashSet<ControladorPalabras> destinos)
    {
        foreach (ControladorPalabras dado in destinos)
        {
            if (dado.TieneTexturasPara(palabra, out char letraFaltante)) continue;

            Debug.LogWarning($"[NIA] Palabra descartada: {palabra} | Falta textura: {letraFaltante} | Dado: {dado.name}", dado);
            return false;
        }

        return true;
    }

    private void MostrarPalabraEnDados(string palabra, string imagen, HashSet<ControladorPalabras> destinos)
    {
        if (grabadoPiedra != null)
            grabadoPiedra.MostrarImagen(imagen);

        foreach (ControladorPalabras dado in destinos)
            dado.MostrarPalabra(palabra);
    }

    private HashSet<ControladorPalabras> ObtenerDados()
    {
        // Sortear una sola palabra y aplicarla a todas las copias.
        // Se mantiene la referencia anterior por compatibilidad con la escena.
        HashSet<ControladorPalabras> destinos = new HashSet<ControladorPalabras>();
        if (controladorPalabras != null) destinos.Add(controladorPalabras);
        if (dadosDeLetras != null)
            foreach (ControladorPalabras dado in dadosDeLetras)
                if (dado != null) destinos.Add(dado);

        if (destinos.Count == 0)
            Debug.LogError("Asigna los seis ControladorPalabras en GameManager > Dados De Letras.", this);
        else if (destinos.Count != CantidadDados)
            Debug.LogWarning("Se esperaban seis dados distintos y hay " + destinos.Count + " asignados.", this);

        return destinos;
    }

    public void RegistrarDadoDerrotado(Transform raizEnemigo)
    {
        if (raizEnemigo == null || dadosDeLetras == null || dadosDerrotados.Count >= CantidadDados) return;

        foreach (ControladorPalabras dado in dadosDeLetras)
        {
            if (dado == null || !dado.transform.IsChildOf(raizEnemigo)) continue;
            if (!dadosDerrotados.Add(raizEnemigo.GetInstanceID())) return;

            Debug.Log($"[NIA] Dados letra derrotados: {dadosDerrotados.Count}/{CantidadDados}", this);
            if (dadosDerrotados.Count == CantidadDados)
            {
                if (grabadoPiedra == null || grabadoPiedra.gameObject.activeSelf)
                    Debug.LogWarning("[NIA] Asigná Grabado Piedra al objeto stone 1 y dejá ese objeto desactivado al iniciar.", this);
                else
                    StartCoroutine(EmergerPiedra(grabadoPiedra.transform));
            }
            return;
        }
    }

    private IEnumerator EmergerPiedra(Transform piedra)
    {
        Vector3 posicionFinal = piedra.position;
        Vector3 posicionEnterrada = posicionFinal - Vector3.up * profundidadPiedra;
        piedra.position = posicionEnterrada;
        piedra.gameObject.SetActive(true);

        float transcurrido = 0f;
        while (transcurrido < duracionAscenso)
        {
            transcurrido += Time.deltaTime;
            float avance = Mathf.SmoothStep(0f, 1f, transcurrido / duracionAscenso);
            piedra.position = Vector3.Lerp(posicionEnterrada, posicionFinal, avance);
            yield return null;
        }
        piedra.position = posicionFinal;
    }
}
