/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Recargar los dados de Nia al acercarse al banco.
 * Explicación: Detecta la entrada en un radio de proximidad y completa
 * el cargador una vez por visita. No requiere colliders de tipo trigger.
 *********************************************************************************************/

using UnityEngine;

[DisallowMultipleComponent]
public class DiceReloadZone : MonoBehaviour
{
    [Header("Arrastrar Nia (componente DiceInventory)")]
    [SerializeField] private DiceInventory playerInventory;
    [Tooltip("Opcional: punto vacío delante del banco. Vacío usa este objeto.")]
    [SerializeField] private Transform reloadCenter;
    [SerializeField, Min(0.1f)] private float reloadRange = 3f;
    [Tooltip("Margen para evitar recargas repetidas al moverse en el borde.")]
    [SerializeField, Min(0.1f)] private float exitMargin = 0.5f;
    [SerializeField] private bool ignoreHeight = true;
    private bool playerInside;

    private void Start()
    {
        if (playerInventory == null)
            Debug.LogWarning("DiceReloadZone: asigná Nia en Player Inventory.", this);
    }

    private void Update()
    {
        if (playerInventory == null || !playerInventory.gameObject.activeInHierarchy) return;
        // No recargar mientras espera la reaparición.
        var health = playerInventory.GetComponent<PlayerHealth>();
        if (health != null && !health.IsAlive) { playerInside = false; return; }
        Vector3 center = reloadCenter != null ? reloadCenter.position : transform.position;
        Vector3 difference = playerInventory.transform.position - center;
        if (ignoreHeight) difference.y = 0f;
        float distance = difference.magnitude;
        if (playerInside)
        {
            if (distance > reloadRange + exitMargin) playerInside = false;
            return;
        }
        if (distance > reloadRange) return;
        playerInside = true;
        playerInventory.ReloadFull();
    }

    private void OnDisable() { playerInside = false; }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = reloadCenter != null ? reloadCenter.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, reloadRange);
    }
}
