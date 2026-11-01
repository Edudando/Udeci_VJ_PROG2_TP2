using UnityEngine;

public class DadoCara : MonoBehaviour
{
    [Tooltip("Arrastra el Mesh Renderer del hijo Cube. Si queda vacío, se busca automáticamente.")]
    [SerializeField] private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propiedades;
    private readonly Texture2D[] texturasCaras = new Texture2D[6];

    public Texture2D ObtenerTexturaCara(int indice)
    {
        return indice >= 0 && indice < texturasCaras.Length ? texturasCaras[indice] : null;
    }

    public bool IntentarSortearCara(out Texture2D textura, out int indiceCara)
    {
        textura = null;
        indiceCara = -1;
        for (int i = 0; i < texturasCaras.Length; i++)
        {
            if (texturasCaras[i] != null) continue;
            Debug.LogError("DadoCara: primero hay que asignar las seis letras con MostrarPalabra().", this);
            return false;
        }
        indiceCara = Random.Range(0, texturasCaras.Length);
        textura = texturasCaras[indiceCara];
        return true;
    }

    private bool PrepararRenderer()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>(true);

        if (meshRenderer == null)
        {
            Debug.LogError("DadoCara: no se encontró el Mesh Renderer de Cube.", this);
            return false;
        }

        MeshFilter filtro = meshRenderer.GetComponent<MeshFilter>();
        if (filtro == null || filtro.sharedMesh == null ||
            filtro.sharedMesh.subMeshCount != 6 || meshRenderer.sharedMaterials.Length != 6)
        {
            Debug.LogError("El dado necesita 6 submallas, una por cara, y 6 materiales. Agregar 6 slots a un cubo común no alcanza.", this);
            return false;
        }

        if (propiedades == null) propiedades = new MaterialPropertyBlock();
        return true;
    }

    // Cada índice corresponde a un material/cara del modelo (0 a 5).
    public void EstablecerLetraEnCara(int indexCara, Texture2D nuevaTextura)
    {
        if (!PrepararRenderer() || nuevaTextura == null || indexCara < 0 || indexCara >= 6)
            return;

        Material material = meshRenderer.sharedMaterials[indexCara];
        if (material == null)
        {
            Debug.LogError("Falta el material de la cara " + (indexCara + 1), this);
            return;
        }

        // Propiedades por slot: no modifica los materiales de otros dados.
        propiedades.Clear();
        meshRenderer.GetPropertyBlock(propiedades, indexCara);
        if (material.HasProperty("_BaseMap"))
        {
            propiedades.SetTexture("_BaseMap", nuevaTextura);
            propiedades.SetVector("_BaseMap_ST", new Vector4(1, 1, 0, 0));
        }
        else if (material.HasProperty("_MainTex"))
        {
            propiedades.SetTexture("_MainTex", nuevaTextura);
            propiedades.SetVector("_MainTex_ST", new Vector4(1, 1, 0, 0));
        }
        else
        {
            Debug.LogError("El material de la cara no admite BaseMap ni MainTex.", this);
            return;
        }
        if (material.HasProperty("_BaseColor")) propiedades.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color")) propiedades.SetColor("_Color", Color.white);
        meshRenderer.SetPropertyBlock(propiedades, indexCara);
        texturasCaras[indexCara] = nuevaTextura;
    }
}
