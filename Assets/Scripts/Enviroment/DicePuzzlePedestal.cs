/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Clase base abstracta para pedestales de puzles.
 * Explicación: Define la estructura general de interacción con pedestales. Implementa IInteractable y maneja la referencia a la barrera asociada para liberarla al resolver el desafío (Polimorfismo).
 *********************************************************************************************/

using UnityEngine;

public abstract class DicePuzzlePedestal : MonoBehaviour, IInteractable
{
    [Header("Configuración de Barrera")]
    [SerializeField] protected BarrierGate targetBarrier;
    protected bool isSolved = false;

    public virtual void Interact(PlayerController3D player)
    {
        if (isSolved) return;

        ExecutePuzzleLogic();
    }

    protected abstract void ExecutePuzzleLogic();

    protected void CompletePuzzle()
    {
        isSolved = true;
        if (targetBarrier != null)
        {
            targetBarrier.UnlockBarrier();
        }
    }

    public virtual string GetInteractionPrompt()
    {
        return isSolved ? "Desafío completado" : "Presiona E para interactuar";
    }
}