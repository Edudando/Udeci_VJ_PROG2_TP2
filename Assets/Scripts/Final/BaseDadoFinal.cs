/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Coloca o recupera la letra de una base del final.
 *********************************************************************************************/
using UnityEngine;

[DisallowMultipleComponent]
public class BaseDadoFinal : MonoBehaviour
{
    [SerializeField] private Transform puntoDado;

    private Texture2D letraApoyada;
    private int indiceCaraApoyada;
    private DadoCara dadoApoyado;

    public char ObtenerLetra(string palabra)
    {
        return letraApoyada != null ? palabra[indiceCaraApoyada] : '\0';
    }

    public void Interactuar(InventarioLetras inventario, DadoCara prefabDadoLetra)
    {
        if (letraApoyada != null)
        {
            // Si la devolución falla, el dado se queda en la base.
            if (!inventario.DevolverAlFinal(letraApoyada, indiceCaraApoyada)) return;

            if (dadoApoyado != null)
            {
                dadoApoyado.gameObject.SetActive(false);
                Destroy(dadoApoyado.gameObject);
            }
            dadoApoyado = null;
            letraApoyada = null;
            return;
        }

        if (inventario.Cantidad == 0) return;
        if (puntoDado == null || prefabDadoLetra == null)
        {
            Debug.LogWarning("Asigná Punto Dado y Prefab Dado Letra para esta base.", this);
            return;
        }

        Texture2D letra = inventario.Caras[0];
        int indiceCara = inventario.IndicesCaras[0];
        DadoCara nuevoDado = CrearDadoVisual(prefabDadoLetra);
        if (nuevoDado == null) return;
        for (int i = 0; i < 6; i++)
        {
            nuevoDado.EstablecerLetraEnCara(i, letra);
            if (nuevoDado.ObtenerTexturaCara(i) == letra) continue;

            // Si el prefab está mal configurado, la letra sigue en el HUD.
            Destroy(nuevoDado.gameObject);
            return;
        }

        if (!inventario.RetirarPrimera())
        {
            Destroy(nuevoDado.gameObject);
            return;
        }

        dadoApoyado = nuevoDado;
        letraApoyada = letra;
        indiceCaraApoyada = indiceCara;
        dadoApoyado.gameObject.SetActive(true);
    }

    private DadoCara CrearDadoVisual(DadoCara prefabDadoLetra)
    {
        MeshRenderer rendererOriginal = prefabDadoLetra.GetComponentInChildren<MeshRenderer>(true);
        MeshFilter filtroOriginal = rendererOriginal != null
            ? rendererOriginal.GetComponent<MeshFilter>()
            : null;
        if (filtroOriginal == null || filtroOriginal.sharedMesh == null ||
            filtroOriginal.sharedMesh.subMeshCount != 6 || rendererOriginal.sharedMaterials.Length != 6)
        {
            Debug.LogError("DadoLetra necesita un Mesh Renderer con seis submallas y seis materiales.", prefabDadoLetra);
            return null;
        }

        // Copia la geometría, sin ejecutar scripts ni física del prefab enemigo.
        Transform origen = prefabDadoLetra.transform;
        Transform mallaOriginal = rendererOriginal.transform;
        GameObject visual = new GameObject("DadoLetra apoyado");
        visual.SetActive(false);
        visual.transform.SetParent(puntoDado, false);
        visual.transform.localPosition = Vector3.Scale(
            origen.InverseTransformPoint(mallaOriginal.position), origen.lossyScale);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = mallaOriginal.lossyScale;

        visual.AddComponent<MeshFilter>().sharedMesh = filtroOriginal.sharedMesh;
        visual.AddComponent<MeshRenderer>().sharedMaterials = rendererOriginal.sharedMaterials;
        return visual.AddComponent<DadoCara>();
    }
}
