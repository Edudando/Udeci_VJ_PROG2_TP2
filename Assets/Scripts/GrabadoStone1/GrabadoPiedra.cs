using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class GrabadoPiedra : MonoBehaviour
{
    private enum VistaGrabado { Imagen, Area, Malla }

    [SerializeField] private Shader shaderGrabado;
    [Tooltip("Centro y orientación del dibujo sobre la roca.")]
    [SerializeField] private Transform puntoGrabado;
    [Tooltip("Ancho del dibujo en unidades de mundo.")]
    [SerializeField, Min(0.01f)] private float ancho = 2f;
    [SerializeField, Range(0f, 1f)] private float intensidad = 0.8f;
    [SerializeField, Range(0.001f, 0.03f)] private float anchoBisel = 0.008f;

    [Tooltip("Area y Malla son vistas de diagnóstico en Play.")]
    [SerializeField] private VistaGrabado vista = VistaGrabado.Imagen;

    private static readonly int ImagenId = Shader.PropertyToID("_Imagen");
    private static readonly int EjeUId = Shader.PropertyToID("_EjeU");
    private static readonly int EjeVId = Shader.PropertyToID("_EjeV");
    private static readonly int VistaId = Shader.PropertyToID("_Vista");
    private static readonly int BiselId = Shader.PropertyToID("_Bisel");
    private static readonly int IntensidadId = Shader.PropertyToID("_Intensidad");
    private static readonly int RevelarColorId = Shader.PropertyToID("_RevelarColor");

    private MeshRenderer piedra;
    private MeshRenderer capaGrabado;
    private Material materialGrabado;
    private Texture2D imagenActual;
    private bool mostrarColor;

    public void MostrarColor(bool visible)
    {
        mostrarColor = visible;
        if (visible && vista != VistaGrabado.Imagen)
            Debug.LogWarning("[NIA] Cambiá Vista de Grabado Piedra a Imagen para ver el color.", this);

        if (materialGrabado == null)
        {
            if (visible)
                Debug.LogWarning("[NIA] La palabra es correcta pero el grabado no está renderizado. Revisá Imagen, Shader Grabado y Punto Grabado.", this);
            return;
        }

        AplicarColor();
    }

    private void AplicarColor()
    {
        if (materialGrabado.HasProperty(RevelarColorId))
            materialGrabado.SetFloat(RevelarColorId, mostrarColor ? 1f : 0f);
        else if (mostrarColor)
            Debug.LogWarning("[NIA] La palabra es correcta, pero Shader Grabado apunta a una versión sin color. Actualizá el GrabadoPiedra.shader asignado.", this);
    }

    public void MostrarImagen(string nombreArchivo)
    {
        imagenActual = null;
        if (!string.IsNullOrWhiteSpace(nombreArchivo))
        {
            string ruta = "images/" + Path.GetFileNameWithoutExtension(nombreArchivo.Trim());
            imagenActual = Resources.Load<Texture2D>(ruta);
            if (imagenActual == null)
                Debug.LogWarning("No se encontró el PNG del grabado en Resources/" + ruta, this);
        }

        ActualizarGrabado();
    }

    private void OnEnable()
    {
        ActualizarGrabado();
    }

    private void LateUpdate()
    {
        ActualizarGrabado();
    }

    private void OnDisable()
    {
        if (capaGrabado != null)
            capaGrabado.enabled = false;
    }

    private void OnDestroy()
    {
        if (capaGrabado != null)
            Destroy(capaGrabado.gameObject);
        if (materialGrabado != null)
            Destroy(materialGrabado);
    }

    private void ActualizarGrabado()
    {
        bool visible = isActiveAndEnabled && imagenActual != null &&
            shaderGrabado != null && puntoGrabado != null;
        if (capaGrabado != null)
            capaGrabado.enabled = visible && piedra.enabled;
        if (!visible)
            return;

        if (capaGrabado == null)
        {
            Mesh malla = GetComponent<MeshFilter>().sharedMesh;
            if (malla == null || malla.subMeshCount == 0)
                return;
            CrearCapa(malla);
        }

        float alto = ancho * imagenActual.height / imagenActual.width;
        Vector3 origen = puntoGrabado.position;
        Vector3 u = puntoGrabado.right / ancho;
        Vector3 v = puntoGrabado.up / alto;
        materialGrabado.SetTexture(ImagenId, imagenActual);
        materialGrabado.SetVector(EjeUId, new Vector4(u.x, u.y, u.z, 0.5f - Vector3.Dot(origen, u)));
        materialGrabado.SetVector(EjeVId, new Vector4(v.x, v.y, v.z, 0.5f - Vector3.Dot(origen, v)));
        materialGrabado.SetFloat(VistaId, (float)vista);
        materialGrabado.SetFloat(BiselId, anchoBisel);
        materialGrabado.SetFloat(IntensidadId, intensidad);
    }

    private void CrearCapa(Mesh malla)
    {
        piedra = GetComponent<MeshRenderer>();
        GameObject capa = new GameObject("Capa de grabado");
        capa.layer = gameObject.layer;
        capa.transform.SetParent(transform, false);
        capa.AddComponent<MeshFilter>().sharedMesh = malla;

        materialGrabado = new Material(shaderGrabado);
        AplicarColor();
        Material[] materiales = new Material[malla.subMeshCount];
        for (int i = 0; i < materiales.Length; i++)
            materiales[i] = materialGrabado;

        capaGrabado = capa.AddComponent<MeshRenderer>();
        capaGrabado.sharedMaterials = materiales;
        capaGrabado.shadowCastingMode = ShadowCastingMode.Off;
        capaGrabado.receiveShadows = false;
        capaGrabado.sortingLayerID = piedra.sortingLayerID;
        capaGrabado.sortingOrder = Mathf.Min(piedra.sortingOrder + 1, short.MaxValue);
        capaGrabado.enabled = piedra.enabled;
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoGrabado == null)
            return;

        float alto = imagenActual != null ? ancho * imagenActual.height / imagenActual.width : ancho;
        Matrix4x4 anterior = Gizmos.matrix;
        Color colorAnterior = Gizmos.color;
        Gizmos.matrix = Matrix4x4.TRS(puntoGrabado.position, puntoGrabado.rotation, Vector3.one);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(ancho, alto, 0f));
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * (ancho * 0.5f));
        Gizmos.matrix = anterior;
        Gizmos.color = colorAnterior;
    }
}
