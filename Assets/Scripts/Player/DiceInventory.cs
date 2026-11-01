/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Administrador del inventario de dados del jugador.
 * Explicación: Gestiona la cantidad actual de dados disponibles para 
 * disparar en combate Ranged o utilizar en pedestales de puzles, 
 * notificando cambios y ofreciendo métodos de consulta.
 *********************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DiceInventory : MonoBehaviour
{
    [Header("Configuración de Inventario / Cargador")]
    [SerializeField, Min(0)] private int currentDiceCount = 0;
    [SerializeField, Min(1)] private int maxDiceCount = 10;

    [Header("Arrastrar Municion_01 a Municion_10 en ese orden")]
    [SerializeField] private GameObject[] ammoVisuals = new GameObject[10];
    [SerializeField] private UnityEvent<int> onAmmoChanged = new UnityEvent<int>();

    public int CurrentDiceCount => currentDiceCount;
    public int MaxDiceCount => maxDiceCount;
    public bool IsFull => currentDiceCount >= maxDiceCount;

    private void Awake()
    {
        maxDiceCount = Mathf.Max(1, maxDiceCount);
        currentDiceCount = Mathf.Clamp(currentDiceCount, 0, maxDiceCount);
    }

    private void OnEnable() { RefreshVisuals(); }
    private void Start() { NotifyChanged(); }

    public bool HasDice() { return currentDiceCount > 0; }

    public void AddDice(int amount)
    {
        if (amount <= 0) return;
        int added = Mathf.Min(amount, maxDiceCount - currentDiceCount);
        if (added <= 0) return;
        currentDiceCount += added;
        NotifyChanged();
        Debug.Log($"Dados cargados. Munición: {currentDiceCount}/{maxDiceCount}", this);
    }

    public bool UseDie()
    {
        if (!HasDice())
        {
            Debug.Log("Sin munición: acercate al banco para recargar.", this);
            return false;
        }
        currentDiceCount--;
        NotifyChanged();
        Debug.Log($"Dado utilizado. Munición: {currentDiceCount}/{maxDiceCount}", this);
        return true;
    }

    public void ReloadFull() { AddDice(maxDiceCount - currentDiceCount); }

    public void RefreshVisuals()
    {
        if (ammoVisuals == null) return;
        for (int i = 0; i < ammoVisuals.Length; i++)
        {
            GameObject visual = ammoVisuals[i];
            if (visual == null) continue;
            // Evitar desactivar al jugador o a un padre suyo por una referencia incorrecta.
            if (visual == gameObject || transform.IsChildOf(visual.transform)) continue;
            bool show = i < currentDiceCount;
            if (visual.activeSelf != show) visual.SetActive(show);
        }
    }

    private void NotifyChanged()
    {
        RefreshVisuals();
        onAmmoChanged.Invoke(currentDiceCount);
    }

    [ContextMenu("Prueba/Recargar completo")]
    private void TestReload() { if (Application.isPlaying) ReloadFull(); }

    [ContextMenu("Prueba/Consumir un dado")]
    private void TestUseDie() { if (Application.isPlaying) UseDie(); }
}

