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
    [SerializeField, Min(0.01f)] private float duracionAscenso = 4f;

    [Header("Cinemática de la piedra")]
    [SerializeField] private Camera camaraPrincipal;
    [SerializeField] private Camera camLaberinto;
    [SerializeField, Min(0f)] private float pausaAntesDeEmerger = 1.25f;
    [SerializeField, Min(0f)] private float pausaAntesDeVolver = 0.5f;

    private bool mostrandoCamLaberinto;
    private Transform piedraEnAscenso;
    private Vector3 posicionFinalPiedra;

    [Header("Recompensa por capturar un dado de letra")]
    [SerializeField] private PlayerHealth saludNia;
    [SerializeField, Min(0)] private int vidasPorCaptura = 1;
    [SerializeField] private GameObject signoCuracion;
    [SerializeField, Min(0.01f)] private float duracionSignoCuracion = 0.8f;


    [Header("Sonidos")]
    [SerializeField] private FMODUnity.EventReference sonidoPiedraEmergiendo;
    [SerializeField] private FMODUnity.StudioEventEmitter emisorAmbiente;

    [System.Serializable]
    private class EnemigosPorDado
    {
        public ControladorPalabras dado;
        public EnemyRanger[] enemigos;
    }

    [Header("EnemyRanger por sector")]
    [Tooltip("Asigná un dado de letra y los EnemyRanger de su sector en cada elemento.")]
    [SerializeField] private EnemigosPorDado[] enemigosPorSector;

    private Coroutine rutinaSignoCuracion;

    [Header("Dificultad actual")]
    public int dificultadSeleccionada = 1;
    private ListaPalabras datosJuego;
    private readonly HashSet<int> dadosDerrotados = new HashSet<int>();

    private void Start()
    {
        if (grabadoPiedra != null && grabadoPiedra.gameObject.activeSelf)
            Debug.LogWarning("[NIA] Desactivá solo el objeto stone 1 en la escena para que empiece oculto.", grabadoPiedra);

        if (camLaberinto != null && camLaberinto != camaraPrincipal)
            camLaberinto.enabled = false;

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

            AplicarRecompensaPorCaptura();
            DerrotarEnemigosDelSector(raizEnemigo);
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

    private void DerrotarEnemigosDelSector(Transform raizDado)
    {
        if (enemigosPorSector == null) return;

        foreach (EnemigosPorDado sector in enemigosPorSector)
        {
            if (sector == null || sector.dado == null || sector.enemigos == null ||
                !sector.dado.transform.IsChildOf(raizDado))
                continue;

            foreach (EnemyRanger enemigo in sector.enemigos)
            {
                if (enemigo == null || !enemigo.IsAlive) continue;
                enemigo.TakeDamage(Mathf.CeilToInt(enemigo.CurrentHealth));
            }
        }
    }

    private void AplicarRecompensaPorCaptura()
    {
        if (saludNia == null || !saludNia.IsAlive || vidasPorCaptura <= 0) return;

        if (saludNia.CurrentHealth < saludNia.MaxHealth)
            saludNia.healing(vidasPorCaptura);

        if (signoCuracion == null || !isActiveAndEnabled) return;

        if (rutinaSignoCuracion != null)
            StopCoroutine(rutinaSignoCuracion);

        signoCuracion.SetActive(true);
        rutinaSignoCuracion = StartCoroutine(OcultarSignoCuracion());
    }

    private IEnumerator OcultarSignoCuracion()
    {
        yield return new WaitForSecondsRealtime(duracionSignoCuracion);
        rutinaSignoCuracion = null;
        if (signoCuracion != null)
            signoCuracion.SetActive(false);
    }

    private IEnumerator EmergerPiedra(Transform piedra)
    {
        piedraEnAscenso = piedra;
        posicionFinalPiedra = piedra.position;
        Vector3 posicionEnterrada = posicionFinalPiedra - Vector3.up * profundidadPiedra;
        piedra.position = posicionEnterrada;
        piedra.gameObject.SetActive(true);

        if (camaraPrincipal != null && camaraPrincipal.isActiveAndEnabled &&
            camLaberinto != null && camLaberinto != camaraPrincipal &&
            camLaberinto.gameObject.activeInHierarchy)
        {
            mostrandoCamLaberinto = true;
            camLaberinto.enabled = true;
            camaraPrincipal.enabled = false;

            if (pausaAntesDeEmerger > 0f)
                yield return new WaitForSecondsRealtime(pausaAntesDeEmerger);

        }
        else
        {
            Debug.LogWarning("[NIA] Asigná Main Camera y Cam_Laberinto al GameManager y dejá activos sus GameObjects.", this);
        }


   

        var ambiente = emisorAmbiente != null
        ? emisorAmbiente.EventInstance
        : default(FMOD.Studio.EventInstance);

        if (ambiente.isValid())
        {
            ambiente.setPaused(true);
        }

        if (!sonidoPiedraEmergiendo.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(sonidoPiedraEmergiendo);
        }

        float duracion = Mathf.Max(0.01f, duracionAscenso);
        float transcurrido = 0f;
        while (piedra != null && transcurrido < duracion)
        {
            yield return null;
            if (piedra == null) break;

            transcurrido += Time.unscaledDeltaTime;
            float avance = Mathf.SmoothStep(0f, 1f, transcurrido / duracion);
            piedra.position = Vector3.Lerp(posicionEnterrada, posicionFinalPiedra, avance);
        }

        if (ambiente.isValid())
        {
            ambiente.setPaused(false);
        }

        if (piedra != null)
            piedra.position = posicionFinalPiedra;
        piedraEnAscenso = null;

        if (mostrandoCamLaberinto && pausaAntesDeVolver > 0f)
            yield return new WaitForSecondsRealtime(pausaAntesDeVolver);

        RestaurarCamaraPrincipal();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        rutinaSignoCuracion = null;
        if (signoCuracion != null)
            signoCuracion.SetActive(false);

        if (piedraEnAscenso != null)
            piedraEnAscenso.position = posicionFinalPiedra;
        piedraEnAscenso = null;
        RestaurarCamaraPrincipal();
    }

    private void RestaurarCamaraPrincipal()
    {
        if (!mostrandoCamLaberinto) return;

        mostrandoCamLaberinto = false;
        if (camLaberinto != null)
            camLaberinto.enabled = false;
        if (camaraPrincipal != null)
            camaraPrincipal.enabled = true;
    }
}
