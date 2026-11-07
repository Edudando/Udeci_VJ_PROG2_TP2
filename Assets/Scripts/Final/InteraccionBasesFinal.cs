/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Detecta la base a la que apunta Nia con FirePoint.
 *********************************************************************************************/
using UnityEngine;

[DisallowMultipleComponent]
public class InteraccionBasesFinal : MonoBehaviour
{
    [SerializeField] private InventarioLetras inventario;
    [SerializeField] private DadoCara prefabDadoLetra;
    [SerializeField] private PuzzlePalabraFinal puzzleFinal;
    [SerializeField] private LayerMask capasDelRayo;
    [SerializeField, Min(0.1f)] private float alcance = 8f;

    public bool IntentarInteractuar(Transform origen)
    {
        if (origen == null) return false;

        if (!Physics.Raycast(origen.position, origen.forward, out RaycastHit impacto,
                alcance, capasDelRayo, QueryTriggerInteraction.Collide))
            return false;

        BaseDadoFinal baseApuntada = impacto.collider.GetComponentInParent<BaseDadoFinal>();
        if (baseApuntada == null || !baseApuntada.isActiveAndEnabled) return false;

        if (inventario == null)
            Debug.LogWarning("Asigná HUD > Inventario en el campo Inventario de Nia.", this);
        else
        {
            baseApuntada.Interactuar(inventario, prefabDadoLetra);
            PuzzlePalabraFinal puzzle = puzzleFinal != null
                ? puzzleFinal
                : baseApuntada.GetComponentInParent<PuzzlePalabraFinal>();
            if (puzzle != null && puzzle.isActiveAndEnabled)
                puzzle.ActualizarSolucion(inventario.PalabraObjetivo);
            else
                Debug.LogWarning("[NIA] Asigná stone 1 con Puzzle Palabra Final en el campo Puzzle Final de Nia.", this);
        }

        // La base consume la interacción incluso si no hay una letra para mover.
        return true;
    }
}
