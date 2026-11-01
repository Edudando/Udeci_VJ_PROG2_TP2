/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Control de apertura y paso de la barrera.
 * Explicación: Gestiona el estado de la barrera en el escenario. Al ser desbloqueada desactiva sus colisiones y representación visual para permitir el progreso del jugador.
 *********************************************************************************************/

using UnityEngine;

public class BarrierGate : MonoBehaviour
{
    [Header("Componentes de la Barrera")]
    [SerializeField] private GameObject barrierVisual;
    [SerializeField] private Collider barrierCollider;

    public void UnlockBarrier()
    {
        if (barrierCollider != null) barrierCollider.enabled = false;
        if (barrierVisual != null) barrierVisual.SetActive(false);

        Debug.Log("¡Barrera invisible liberada!");
    }
}