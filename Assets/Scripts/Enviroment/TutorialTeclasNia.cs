using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public sealed class TutorialTeclasNia : MonoBehaviour
{
    private const int DadosNecesarios = 6;

    [Header("Aviso al empezar")]
    [SerializeField, TextArea(3, 5)] private string mensajeInicio =
        "<b>CONTROLES DE NIA</b>\n" +
        "Flechas arriba y abajo: avanzar y retroceder\n" +
        "Flechas izquierda y derecha: girar\n" +
        "<b>F</b>: disparar";

    [SerializeField, Min(1f)] private float segundosInicio = 8f;

    [Header("Aviso al entrar al laberinto")]
    [SerializeField] private InventarioLetras inventarioLetras;
    [SerializeField, TextArea(3, 5)] private string mensajeLaberinto =
        "<b>COLOCÁ LAS LETRAS</b>\n" +
        "Apuntá a una base y presioná <b>E</b> para colocar una letra.\n" +
        "Presioná <b>E</b> otra vez para recuperarla.";

    [SerializeField, Min(1f)] private float segundosLaberinto = 8f;

    private GameObject lienzo;
    private GameObject panel;
    private Text texto;
    private Coroutine ocultacion;
    private bool laberintoMostrado;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;
    }

    private void Start()
    {
        CrearInterfaz();
        MostrarAviso(mensajeInicio, segundosInicio);
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (!isActiveAndEnabled || laberintoMostrado || !otro.CompareTag("Player"))
            return;

        if (inventarioLetras == null)
            inventarioLetras = FindFirstObjectByType<InventarioLetras>();
        if (inventarioLetras == null || inventarioLetras.Cantidad != DadosNecesarios)
            return;

        laberintoMostrado = true;
        MostrarAviso(mensajeLaberinto, segundosLaberinto);
    }

    private void MostrarAviso(string mensaje, float segundos)
    {
        if (ocultacion != null)
            StopCoroutine(ocultacion);

        texto.text = mensaje;
        panel.SetActive(true);
        ocultacion = StartCoroutine(OcultarAviso(segundos));
    }

    private IEnumerator OcultarAviso(float segundos)
    {
        yield return new WaitForSecondsRealtime(segundos);
        panel.SetActive(false);
        ocultacion = null;
    }

    private void OnDisable()
    {
        if (ocultacion != null)
        {
            StopCoroutine(ocultacion);
            ocultacion = null;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (lienzo != null)
            Destroy(lienzo);
    }

    private void CrearInterfaz()
    {
        lienzo = new GameObject("Lienzo Tutorial Nia", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = lienzo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler escala = lienzo.GetComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1280f, 720f);
        escala.matchWidthOrHeight = 0f;

        panel = new GameObject("Aviso", typeof(RectTransform), typeof(Image));
        RectTransform rectPanel = panel.GetComponent<RectTransform>();
        rectPanel.SetParent(lienzo.transform, false);
        rectPanel.anchorMin = new Vector2(0.5f, 1f);
        rectPanel.anchorMax = rectPanel.anchorMin;
        rectPanel.pivot = rectPanel.anchorMin;
        rectPanel.anchoredPosition = new Vector2(0f, -22f);
        rectPanel.sizeDelta = new Vector2(640f, 126f);

        Image fondo = panel.GetComponent<Image>();
        fondo.color = new Color(0.19f, 0.12f, 0.07f, 0.94f);
        fondo.raycastTarget = false;

        GameObject etiqueta = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        RectTransform rectTexto = etiqueta.GetComponent<RectTransform>();
        rectTexto.SetParent(rectPanel, false);
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = new Vector2(16f, 8f);
        rectTexto.offsetMax = new Vector2(-16f, -8f);

        texto = etiqueta.GetComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.color = new Color(1f, 0.96f, 0.86f, 1f);
        texto.alignment = TextAnchor.MiddleCenter;
        texto.fontSize = 22;
        texto.resizeTextForBestFit = true;
        texto.resizeTextMinSize = 14;
        texto.resizeTextMaxSize = 22;
        texto.supportRichText = true;
        texto.raycastTarget = false;
    }
}
